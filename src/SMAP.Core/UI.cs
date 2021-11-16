using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ParadoxNotion.Serialization;
using ChaCustom;

using HarmonyLib;

using KKAPI.Maker;

namespace SMAP
{
	public partial class SMAP
	{
		private static CanvasGroup _cgAccessoryTop, _cgAcsParentWindow;
		private static SMAPUI _makerConfigWindow = null;

		private static Toggle _tglSMAP = null;

		internal class SMAPUI : MonoBehaviour
		{
			private ChaControl _chaCtrl = null;
			private int _windowRectID;
			private Vector2 _windowSize = new Vector2(330, 250);
			private Vector2 _windowPos = new Vector2(525, 636);
			private Vector2 _resScaleFactor = Vector2.one;
			private Vector2 _ScreenRes = Vector2.zero;
			private Matrix4x4 _resScaleMatrix;
			private Rect _windowRect;
			private bool _hasFocus = false;
			private Texture2D _windowBGtex = null;
			private const int _singleItemHeight = 23;

			private static List<string> _boneSuggestions = new List<string>();

			private bool _searchFieldValueChanged = false;
			private string _searchFieldValue = "";
			private string SearchFieldValue
			{
				get => _searchFieldValue;
				set
				{
					if (_searchFieldValue != value)
					{
						_searchFieldValue = value;
						_searchFieldValueChanged = true;
					}
				}
			}

			private static List<string> _bookmark = new List<string>();
			private static string _bookmarkFile = Path.Combine(BepInEx.Paths.ConfigPath, "SMAPBookmark.json");

			private Vector2 _boneScrollPos = Vector2.zero;
			private static int _curSlot => AccessoriesApi.SelectedMakerAccSlot;

			private void Awake()
			{
				DontDestroyOnLoad(this);
				//enabled = false;
				ChangeRes();
				_windowRect = new Rect(_windowPos.x, _windowPos.y, _windowSize.x, _windowSize.y);

				GameObject _base = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop/AcsParentWindow");
				Toggle _origin = Traverse.Create(_base.GetComponent<CustomAcsParentWindow>()).Field("tglParent").GetValue<Toggle[]>()[0];
				Transform _copy = Instantiate(_origin.transform, _base.transform.Find("grpParent").transform, true);
				RectTransform _copy_rt = _copy.GetComponent<RectTransform>();

				_copy.name = "imgRbColSMAP";
				_copy_rt.offsetMin = new Vector2(284f, -604f);
				_copy_rt.offsetMax = new Vector2(372f, -584f);
				_copy.GetComponentInChildren<TextMeshProUGUI>().text = "S.M.A.P";
				_tglSMAP = _copy.GetComponent<Toggle>();
				_tglSMAP.onValueChanged.RemoveAllListeners();
#if KK
				_windowBGtex = JetPack.UI.MakePlainTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.5f, 0.5f, 0.5f, 1f));
#else
				_windowBGtex = JetPack.UI.MakePlainTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.2f, 0.2f, 0.2f, 1f));
#endif
				_chaCtrl = CustomBase.Instance.chaCtrl;
				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);

				_cgAccessoryTop = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/04_AccessoryTop").GetComponent<CanvasGroup>();
				_cgAcsParentWindow = _base.GetComponent<CanvasGroup>();

				AccessoryParentKey = Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey)).Skip(1).Where(x => !x.StartsWith("a_n_")).ToList();
				_boneSuggestions = AccessoryParentKey.ToList();

				LoadBookmark();
			}

			private static void SaveBookmark()
			{
				string json = JSONSerializer.Serialize(typeof(List<string>), _bookmark, true);
				File.WriteAllText(_bookmarkFile, json);
			}

			private static void LoadBookmark()
			{
				if (!File.Exists(_bookmarkFile)) return;
				_bookmark = JSONSerializer.Deserialize<List<string>>(File.ReadAllText(_bookmarkFile));
			}

			private static bool _display()
			{
				if (CustomBase.Instance.customCtrl.hideFrontUI) return false;
				if (JetPack.Toolbox.SceneIsOverlap()) return false;
				if (!JetPack.Toolbox.SceneAddSceneName().IsNullOrEmpty() && JetPack.Toolbox.SceneAddSceneName() != "CustomScene") return false;
				if (_cgAccessoryTop != null && _cgAcsParentWindow != null)
					return (_cgAccessoryTop.alpha > 0 && _cgAcsParentWindow.alpha > 0);

				return false;
			}

			private ChaFileAccessory.PartsInfo GetPartsInfo()
			{
				if (_chaCtrl == null)
					return null;
#if KK
				if (_curSlot < 20)
#endif
				{
					if (_chaCtrl.chaFile.coordinate.ElementAtOrDefault(_chaCtrl.fileStatus.coordinateType) == null)
						return null;
					return _chaCtrl.chaFile.coordinate[_chaCtrl.fileStatus.coordinateType].accessory.parts.ElementAtOrDefault(_curSlot);
				}
#if KK
				return MoreAccessoriesKOI.MoreAccessories._self._charaMakerData.nowAccessories.ElementAtOrDefault(_curSlot - 20);
#endif
			}

			private void OnGUI()
			{
				if (!_display()) return;

				if (_ScreenRes.x != Screen.width || _ScreenRes.y != Screen.height)
					ChangeRes();

				GUI.matrix = _resScaleMatrix;

				if (_searchFieldValueChanged)
				{
					_searchFieldValueChanged = false;
					_boneSuggestions = AccessoryParentKey.ToList();
					if (!_searchFieldValue.IsNullOrEmpty())
						_boneSuggestions = _boneSuggestions.Where(x => x.Contains(_searchFieldValue)).ToList();
				}

				GUIStyle _windowSolid = new GUIStyle(GUI.skin.window);
				Texture2D _onNormalBG = _windowSolid.onNormal.background;
				_windowSolid.normal.background = _onNormalBG;

				GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "", _windowSolid);

				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = false;

				if (_hasFocus && JetPack.UI.GetResizedRect(_windowRect).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			internal void ChangeRes()
			{
				_ScreenRes.x = Screen.width;
				_ScreenRes.y = Screen.height;
				_resScaleFactor.x = _ScreenRes.x / 1600;
				_resScaleFactor.y = _ScreenRes.y / 900;
				_resScaleMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(_resScaleFactor.x, _resScaleFactor.y, 1));
			}

			private void DrawWindowContents(int _windowID)
			{
				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = true;
#if !KK
				GUI.backgroundColor = Color.grey;
#endif
				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 23), $"S.M.A.P - Slot{_curSlot + 1:00}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

				GUIStyle _buttonActive = new GUIStyle(GUI.skin.button);
				_buttonActive.normal.textColor = Color.cyan;
				_buttonActive.hover.textColor = Color.cyan;

				ChaFileAccessory.PartsInfo _partInfo = GetPartsInfo();

				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(GUILayout.Width(200));
					{
						GUILayout.BeginHorizontal();
						{
							SearchFieldValue = GUILayout.TextField(SearchFieldValue);
							if (SearchFieldValue.IsNullOrEmpty())
								GUI.enabled = false;

							if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
							{
								if (_bookmark.IndexOf(SearchFieldValue) < 0)
								{
									_bookmark.Add(SearchFieldValue);
									SaveBookmark();
								}
							}

							if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
							{
								SearchFieldValue = "";
							}
							GUI.enabled = true;
						}
						GUILayout.EndHorizontal();

						_boneScrollPos = GUILayout.BeginScrollView(_boneScrollPos, GUI.skin.box);
						{
							GUILayout.BeginVertical();
							{
								int _leftItemCount = Mathf.FloorToInt(_boneScrollPos.y / _singleItemHeight);
								int _shownItemCount = 8;

								List<string> _filtered = _boneSuggestions.ToList();
								if (_filtered.Count > (_shownItemCount - 1))
								{
									if (_boneSuggestions.Count - _leftItemCount < _shownItemCount)
										_leftItemCount = _boneSuggestions.Count - _shownItemCount;

									GUILayout.Space(_leftItemCount * _singleItemHeight);
									_filtered = _boneSuggestions.Skip(_leftItemCount).Take(_shownItemCount).ToList();
								}

								foreach (string _name in _filtered)
								{
									GUILayout.BeginHorizontal();
									if (_name == _partInfo?.parentKey)
										GUILayout.Label(_name, _buttonActive);
									else
									{
										if (GUILayout.Button(_name))
										{
											if (_partInfo == null) return;

											_chaCtrl.ChangeAccessoryParent(_curSlot, _name);
											_partInfo.parentKey = _name;
											CustomBase.Instance.updateCustomUI = true;
										}
									}
									GUILayout.EndHorizontal();
								}

								if (_filtered.Count > (_shownItemCount - 1))
								{
									int _rightItemCount = Mathf.Max(0, _boneSuggestions.Count - (_leftItemCount + _shownItemCount));
									GUILayout.Space(_rightItemCount * _singleItemHeight);
								}
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndScrollView();
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
					{
						foreach (string keyword in _bookmark.ToList())
						{
							GUILayout.BeginHorizontal();
							if (GUILayout.Button(keyword))
								SearchFieldValue = keyword;
							if (GUILayout.Button("X", GUILayout.Width(20)))
							{
								_bookmark.Remove(keyword);
								SaveBookmark();
							}
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}
