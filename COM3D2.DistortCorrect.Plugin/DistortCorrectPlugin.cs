using System.Collections.Generic;
using System.Linq;
using CM3D2.ExternalSaveData.Managed;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using AddModsSliderPlugin = CM3D2.AddModsSlider.Plugin.AddModsSlider;
using MaidVoicePitchPlugin = CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch;

namespace COM3D2.DistortCorrect.Plugin;

[PluginName("DistortCorrect")]
[PluginVersion("0.4.0.6")]
class DistortCorrectPlugin : PluginBase {
	private bool _sceneChanged = false;

	private void Start() {
		var emp = new AddModsSliderPlugin.ExternalModsParam("LIMBSFIX", "手足の歪み修正", true, "toggle", "FACE_OFF");
		AddModsSliderPlugin.AddExternalModsParam(emp);
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		_sceneChanged = true;
	}

	private void Update() {
		if (_sceneChanged) {
			// BoneMorph_.Blend 処理終了後のコールバック
			CM3D2.MaidVoicePitch.Managed.Callbacks.BoneMorph_.Blend.Callbacks[MaidVoicePitchPlugin.PluginName] = BoneMorph_BlendCallback;
			_sceneChanged = false;
		}
	}

	/// <summary>
	/// BoneMorph_.Blend の処理終了後に呼ばれるコールバック。
	/// 初期化、設定変更時のみ呼び出される。
	/// ボーンのブレンド処理が行われる際、拡張スライダーに関連する補正は基本的にここで行う。
	/// 毎フレーム呼び出されるわけではないことに注意
	/// </summary>
	private void BoneMorph_BlendCallback(BoneMorph_ boneMorph_) {
		if (TryGetMaid(boneMorph_, out var maid)) {
			var wideSlider = ExSaveData.GetBool(maid, MaidVoicePitchPlugin.PluginName, "WIDESLIDER", false);
			var limbFix = ExSaveData.GetBool(maid, MaidVoicePitchPlugin.PluginName, "LIMBSFIX", false);
			var enable = wideSlider && limbFix;
			if (enable) {
				WideSlider(maid);
			} else {
				MaidVoicePitchPlugin.WideSlider(maid);
			}
			//MaidVoicePitchPlugin.EyeBall(maid);
			//if (SceneManager.GetActiveScene().name != "ScenePhotoMode") {
			//    if (maid.body0 != null && maid.body0.isLoadedBody) {
			//        IKPreInit(maid);
			//        maid.body0.IKCtrl.Init();
			//    }
			//}
		}
	}

	private void IKPreInit(Maid maid) {
		var fbikc = maid.body0.IKCtrl;
		if (fbikc.m_Mouth) DestroyImmediate(fbikc.m_Mouth.gameObject);
		if (fbikc.m_NippleL) DestroyImmediate(fbikc.m_NippleL.gameObject);
		if (fbikc.m_NippleR) DestroyImmediate(fbikc.m_NippleR.gameObject);
	}

	private static readonly KeyValuePair<string, string>[] BoneAndPropNameList = {
		new("Bip01 ? Thigh_SCL_", "THISCL"),     // 下半身
		new("Bip01 ? Calf_SCL_",  "THISCL"),     // 下半身
		new("Bip01 ? Foot",       "THISCL"),     // 下半身
		new("momotwist_?",        "THISCL"),     // 下半身
		new("momotwist_?",        "MTWSCL"),     // ももツイスト
		new("momoniku_?",         "MMNSCL"),     // もも肉
		new("Bip01 Pelvis_SCL_",  "PELSCL"),     // 骨盤
		new("Hip_?",              "PELSCL"),     // 骨盤
		new("Hip_?",              "HIPSCL"),     // 骨盤
		new("Bip01 ? Thigh_SCL_", "THISCL2"),    // 膝
		new("Bip01 ? Calf_SCL_",  "CALFSCL"),    // 膝下
		new("Bip01 ? Foot",       "CALFSCL"),    // 膝下
		new("Bip01 ? Foot",       "FOOTSCL"),    // 足首より下
		new("Skirt",              "SKTSCL"),     // スカート
		new("Bip01 Spine_SCL_",   "SPISCL"),     // 胴(下腹部周辺)
		new("Bip01 Spine0a_SCL_", "S0ASCL"),     // 胴0a(腹部周辺)
		new("Bip01 Spine1_SCL_",  "S1_SCL"),     // 胴1_(みぞおち周辺)
		new("Bip01 Spine1a_SCL_", "S1ASCL"),     // 胴1a(首・肋骨周辺)
		new("Bip01 Spine1a",      "S1ABASESCL"), // 胴1a(胸より上)※頭に影響有り

		new("Kata_?",             "KATASCL"),    // 肩
		new("Mune_?",             "MUNESCL"),    // 胸
		new("Mune_?_sub",         "MUNESUBSCL"), // 胸サブ
		new("Bip01 Neck_SCL_",    "NECKSCL"),    // 首
	};

	//この配列に記載があるボーンは頭に影響を与えずにTransformを反映させる。
	//ただしボディに繋がっている中のアレは影響を受ける。
	private static readonly string[] IgnoreHeadBones = {
		"Bip01 Spine1a",
	};

	private void WideSlider(Maid maid) {
		var tbody = maid.body0;
		if (tbody?.bonemorph?.bones == null) {
			return;
		}

		var boneMorph = tbody.bonemorph;

		// スケール変更するボーンのリスト
		var boneScales = new Dictionary<string, Vector3>();

		// ポジション変更するボーンのリスト
		var bonePositions = MaidVoicePitchPlugin.GetBonePositions(maid);

		// ポジション変更するボーンのリスト
		var bonePositionRates = new Dictionary<string, Vector3>();

		SetBoneScales(boneScales, maid, "CLVSCL", BodyBone.ClavicleScales);
		SetBoneScales(boneScales, maid, "UPARMSCL", BodyBone.UpperArmScales);
		SetBoneScales(boneScales, maid, "FARMSCL", BodyBone.ForeArmScales);
		SetBoneScales(boneScales, maid, "HANDSCL", BodyBone.HandScales);

		SetBoneScales(bonePositionRates, maid, "CLVSCL", BodyBone.ClaviclePositions);
		SetBoneScales(bonePositionRates, maid, "UPARMSCL", BodyBone.UpperArmPositions);
		SetBoneScales(bonePositionRates, maid, "FARMSCL", BodyBone.ForeArmPositions);
		SetBoneScales(bonePositionRates, maid, "HANDSCL", BodyBone.HandPositions);

		// スケール変更するボーンをリストに一括登録
		SetBoneScales(boneScales, maid, BoneAndPropNameList);

		// 元々尻はPELSCLに連動していたが単体でも設定できるようにする
		// ただし元との整合性をとるため乗算する
		//Vector3 pelScl = new Vector3(
		//    ExSaveData.GetFloat(maid, PluginName, "PELSCL.height", 1f),
		//    ExSaveData.GetFloat(maid, PluginName, "PELSCL.depth", 1f),
		//    ExSaveData.GetFloat(maid, PluginName, "PELSCL.width", 1f));
		//Vector3 hipScl = new Vector3(
		//    ExSaveData.GetFloat(maid, PluginName, "HIPSCL.height", 1f) * pelScl.x,
		//    ExSaveData.GetFloat(maid, PluginName, "HIPSCL.depth", 1f) * pelScl.y,
		//    ExSaveData.GetFloat(maid, PluginName, "HIPSCL.width", 1f) * pelScl.z);
		//boneScale["Hip_L"] = hipScl;
		//boneScale["Hip_R"] = hipScl;

		Transform tEyePosL = null;
		Transform tEyePosR = null;

		for (var i = boneMorph.bones.Count - 1; i >= 0; i--) {
			var boneMorphLocal = boneMorph.bones[i];

			MaidVoicePitchPlugin.GetBoneProperties(boneMorph, boneMorphLocal, out var scale, out var position);

			var linkT = boneMorphLocal.linkT;
			if (linkT == null) {
				continue;
			}

			var name = linkT.name;

			if (name != null) {
				if (name.Contains("Thigh_SCL_")) {
					boneMorph.SnityouOutScale = Mathf.Pow(scale.x, 0.9f);
				}

				// リストに登録されているボーンのスケール設定
				if (boneScales.TryGetValue(name, out var boneScale)) {
					scale = Vector3.Scale(scale, boneScale);
				}

				// リストに登録されているボーンのポジション設定
				if (bonePositions.TryGetValue(name, out var bonePosition)) {
					position += bonePosition;
				}

				// リストに登録されているボーンのポジション設定
				if (bonePositionRates.TryGetValue(name, out var bonePositionRate)) {
					position = Vector3.Scale(position, bonePositionRate);
				}
			}

			MaidVoicePitchPlugin.UpdateBreastPositions(tbody);

			// ignoreHeadBonesに登録されている場合はヒラエルキーを辿って頭のツリーを無視
			if (name != null) {
				if (!(IgnoreHeadBones.Contains(name) && CMT.SearchObjObj(maid.body0.m_Bones.transform.Find("Bip01"), linkT))) {
					linkT.localScale = scale;
				}
				linkT.localPosition = position;
			}

			if (name == "Eyepos_L") {
				tEyePosL = linkT;
			}
			if (name == "Eyepos_R") {
				tEyePosR = linkT;
			}
		}

		MaidVoicePitchPlugin.RotateEyes(maid, tEyePosL, tEyePosR);
		MaidVoicePitchPlugin.MorphBones(boneMorph);
	}

	private static void SetBoneScales(Dictionary<string, Vector3> boneScales, Maid maid, KeyValuePair<string, string>[] boneAndPropNameList) {
		foreach (var item in boneAndPropNameList) {
			SetBoneScale(boneScales, item.Key, maid, item.Value);
		}
	}

	private static void SetBoneScales(Dictionary<string, Vector3> boneScales, Maid maid, string tag, List<string> bones) {
		foreach (var boneName in bones) {
			SetBoneScale(boneScales, boneName, maid, tag);
		}
	}

	private static void SetBoneScale(Dictionary<string, Vector3> boneScales, string boneName, Maid maid, string propName) {
		if (boneName.Contains("?")) {
			var boneNameL = boneName.Replace('?', 'L');
			var boneNameR = boneName.Replace('?', 'R');
			boneScales[boneNameL] = GetBoneScale(boneNameL);
			boneScales[boneNameR] = boneScales[boneNameL];
		} else {
			boneScales[boneName] = GetBoneScale(boneName);
		}

		Vector3 GetBoneScale(string boneName) {
			var boneScale = MaidVoicePitchPlugin.GetBoneScale(maid, propName);
			var baseScale = boneScales.TryGetValue(boneName, out var scale) ? scale : Vector3.one;
			return Vector3.Scale(baseScale, boneScale);
		}
	}

	// BoneMorph_を手がかりに、Maidを得る
	private static bool TryGetMaid(BoneMorph_ boneMorph_, out Maid maid) {
		maid = null;
		if (boneMorph_ == null) {
			return false;
		}
		maid = GetMaids().FirstOrDefault(e => e.body0?.bonemorph != null && e.body0.bonemorph == boneMorph_);
		return maid;
	}

	private static IEnumerable<Maid> GetMaids() {
		var characterManager = GameMain.Instance.CharacterMgr;
		for (var i = 0; i < characterManager.GetStockMaidCount(); i++) {
			yield return characterManager.GetStockMaid(i);
		}
		foreach (var npcMaid in characterManager.m_listStockNpcMaid) {
			yield return npcMaid;
		}
	}
}
