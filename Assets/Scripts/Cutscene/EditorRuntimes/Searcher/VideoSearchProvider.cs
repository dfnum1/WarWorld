#if UNITY_EDITOR
/********************************************************************
生成日期:	11:9:2025
类    名: 	VideoSearchProvider
作    者:	HappLI
描    述:	视频选择搜索
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public class VideoSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		public class VideoInfo
		{
			public int video;
			public string codeName;
			public string videoName;
			public string videoFile;
			public bool streamAsset;
		}
		private List<SearchTreeEntry> _entries;
		private Action<string, bool> _onSelect;
		static string ms_lastMd5 = null;
		static Dictionary<int, VideoInfo> ms_vVideos = new();
        static Dictionary<string, VideoInfo> ms_vCodeNameVideos = new();
        static bool ms_bLoaded = false;
		public static Dictionary<int, VideoInfo> GetVideos(string videoCfgPath = "Assets/Res/LuaTable/VideoTable.txt")
		{
			string streamAssetVideo = "Assets/StreamingAssets/Video";
			List<int> unExistVideos = new List<int>();
			foreach (var db in ms_vVideos)
			{
				if(db.Value.streamAsset)
				{
					if(!File.Exists(Path.Combine("Assets/StreamingAssets", db.Value.videoName)))
					{
						unExistVideos.Add(db.Key);
						ms_vCodeNameVideos.Remove(db.Value.codeName);
                    }
				}
			}
			foreach(var db in unExistVideos)
			{
				ms_vVideos.Remove(db);

            }
			if(Directory.Exists(streamAssetVideo))
			{
				string fullPath = Path.GetFullPath(streamAssetVideo).Replace("\\", "/");
				fullPath += "/";
				var files = Directory.GetFiles(streamAssetVideo, "*.mp4", SearchOption.AllDirectories);
				for(int i =0; i < files.Length; ++i)
				{
					string name = files[i].Replace("\\", "/").Replace(fullPath, "").Replace("Assets/StreamingAssets", "");
					if (name.StartsWith("/")) name = name.Substring(1);
                    int videoId = Animator.StringToHash(name);
					VideoInfo video = new VideoInfo();
					video.video = videoId;
					video.codeName = name;
					video.videoName = name;
					video.videoFile = files[i].Replace("\\", "/");
					video.streamAsset = true;
					ms_vVideos[videoId] = video;
					ms_vCodeNameVideos[name] = video;
                }
			}

			if (!File.Exists(videoCfgPath))
			{
				return ms_vVideos;
			}
			string text = File.ReadAllText(videoCfgPath);
			if (string.IsNullOrEmpty(ms_lastMd5))
			{
				ms_lastMd5 = GetFileMD5(videoCfgPath);
				ms_bLoaded = false;
			}
			else
			{
				if (ms_lastMd5.CompareTo(GetFileMD5(videoCfgPath)) != 0)
					ms_bLoaded = false;
			}
			if (!ms_bLoaded)
			{
				ms_bLoaded = true;

				// 1. 解析 Video.Code 映射
				var codeDict = new Dictionary<int, string>();
				var codeBlockMatch = Regex.Match(text, @"Video\.Code\s*=\s*\{([^}]*)\}", RegexOptions.Singleline);
				if (codeBlockMatch.Success)
				{
					var codeBlock = codeBlockMatch.Groups[1].Value;
					var codeMatches = Regex.Matches(codeBlock, @"([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(\d+)");
					foreach (Match m in codeMatches)
					{
						string codeName = m.Groups[1].Value;
						int videoId = int.Parse(m.Groups[2].Value);
						codeDict[videoId] = codeName;
					}
				}

				// 2. 解析 Video.Data
				var dataMatches = Regex.Matches(text, @"Video\s*=\s*(\d+),\s*VideoName\s*=\s*[""']([^""']+)[""']", RegexOptions.Singleline);
				foreach (Match m in dataMatches)
				{
					int videoId = int.Parse(m.Groups[1].Value);
					string videoName = m.Groups[2].Value;
					string codeName = codeDict.TryGetValue(videoId, out var name) ? name : "";

                    var videoInfo = new VideoInfo
                    {
                        video = videoId,
                        streamAsset = false,
                        codeName = codeName,
                        videoName = videoName
                    };
                    var files = AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(videoName));
                    for (int i = 0; i < files.Length; ++i)
                    {
                        string videoFile = AssetDatabase.GUIDToAssetPath(files[i]);
                        if (Framework.ED.EditorUtils.IsMp4ByHeader(videoFile))
						{
							videoInfo.videoFile = videoFile.Replace("\\", "/");
                            break;
						}
                    }
                    ms_vVideos[videoId] = videoInfo;
					ms_vCodeNameVideos[codeName] = videoInfo;
                }
			}
			return ms_vVideos;
		}
        //------------------------------------------------------
		public static VideoInfo GetVideoByCodeName(string videoCodeName)
		{
			GetVideos();
			if (ms_vCodeNameVideos.TryGetValue(videoCodeName, out var videoInfo))
				return videoInfo;
			return null;
        }
        //------------------------------------------------------
        static string GetFileMD5(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        //------------------------------------------------------
        public void Init(Action<string, bool> onSelect)
		{
			_onSelect = onSelect;
			_entries = null; // 重新加载
		}
		//------------------------------------------------------
		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			if (_entries != null)
				return _entries;

			_entries = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent("Video"), 0)
			};
			var assetList = GetVideos();

			// 路径分级缓存，key为完整路径，value为SearchTreeEntry
			var groupDict = new Dictionary<string, SearchTreeEntry>
				{
					{ "", _entries[0] } // 根节点
				};

			if (assetList != null)
			{
				foreach (var info in assetList)
				{
					var pathParts = info.Value.codeName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
					string curPath = "";
					int level = 1;

					// 逐级创建分组
					for (int i = 0; i < pathParts.Length - 1; i++)
					{
						string part = pathParts[i];
						string parentPath = curPath;
						curPath = string.IsNullOrEmpty(curPath) ? part : $"{curPath}/{part}";

						if (!groupDict.ContainsKey(curPath))
						{
							var groupEntry = new SearchTreeGroupEntry(new GUIContent(part), level);
							_entries.Add(groupEntry);
							groupDict[curPath] = groupEntry;
						}
						level++;
					}

					// 添加节点
					string prefabName = pathParts[pathParts.Length - 1];
					var entry = new SearchTreeEntry(new GUIContent(prefabName))
					{
						level = level,
						userData = info.Value // 选择时返回prefabName
					};
					_entries.Add(entry);
				}
			}

			return _entries;
		}
		//------------------------------------------------------
		public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
		{
			if (entry.userData is VideoInfo name)
			{
				_onSelect?.Invoke(name.codeName, name.streamAsset);
				return true;
			}
			return false;
		}
	}
}
#endif