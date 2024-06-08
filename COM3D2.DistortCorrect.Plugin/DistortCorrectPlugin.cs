using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using CM3D2.AddModsSlider.Plugin;
using CM3D2.ExternalSaveData.Managed;
using CM3D2.MaidVoicePitch.Plugin;

namespace COM3D2.DistortCorrect.Plugin;

[PluginName("DistortCorrect")]
[PluginVersion("0.4.0.6")]
class DistortCorrectPlugin : PluginBase {
	private bool _sceneChanged = false;

	private void Start() {
		var emp = new AddModsSlider.ExternalModsParam("LIMBSFIX", "手足の歪み修正", true, "toggle", "FACE_OFF");
		AddModsSlider.AddExternalModsParam(emp);
	}

	private void OnLevelWasLoaded(int level) {
		_sceneChanged = true;
	}

	private void Update() {
		if (_sceneChanged) {
			// BoneMorph_.Blend 処理終了後のコールバック
			CM3D2.MaidVoicePitch.Managed.Callbacks.BoneMorph_.Blend.Callbacks["CM3D2.MaidVoicePitch"] = BoneMorph_BlendCallback;
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
			var wideSlider = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "WIDESLIDER", false);
			var limbFix = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "LIMBSFIX", false);
			var enable = wideSlider && limbFix;
			if (enable) {
				WideSlider(maid);
			} else {
				CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch.WideSlider(maid);
			}
			//CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch.EyeBall(maid);
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

	// WideSlider
	private static string PluginName { get { return "CM3D2.MaidVoicePitch"; } }

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

	private void WideSlider(Maid maid) {
		TBody tbody = maid.body0;
		if (tbody == null || tbody.bonemorph == null || tbody.bonemorph.bones == null) {
			return;
		}
		BoneMorph_ boneMorph_ = tbody.bonemorph;

		// スケール変更するボーンのリスト
		Dictionary<string, Vector3> boneScale = new Dictionary<string, Vector3>();

		// ポジション変更するボーンのリスト
		Dictionary<string, Vector3> bonePosition = new Dictionary<string, Vector3>();

		// ポジション変更するボーンのリスト
		Dictionary<string, Vector3> bonePositionRate = new Dictionary<string, Vector3>();

		//この配列に記載があるボーンは頭に影響を与えずにTransformを反映させる。
		//ただしボディに繋がっている中のアレは影響を受ける。
		string[] ignoreHeadBones = new string[] { "Bip01 Spine1a" };

		float eyeAngAngle;
		float eyeAngX;
		float eyeAngY;
		{
			float ra = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.angle", 0f);
			float rx = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.x", 0f);
			float ry = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.y", 0f);

			rx += -9f;
			ry += -17f;

			rx /= 1000f;
			ry /= 1000f;

			eyeAngAngle = ra;
			eyeAngX = rx;
			eyeAngY = ry;
		}

		Vector3 thiScl = new Vector3(
			1.0f,
			ExSaveData.GetFloat(maid, PluginName, "THISCL.depth", 1f),
			ExSaveData.GetFloat(maid, PluginName, "THISCL.width", 1f));

		Vector3 thiPosL;
		Vector3 thiPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "THIPOS.x", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "THIPOS.z", 0f);
			float dy = 0.0f;
			thiPosL = new Vector3(dy, dz / 1000f, -dx / 1000f);
			thiPosR = new Vector3(dy, dz / 1000f, dx / 1000f);
		}
		bonePosition["Bip01 L Thigh"] = thiPosL;
		bonePosition["Bip01 R Thigh"] = thiPosR;

		Vector3 thi2PosL;
		Vector3 thi2PosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "THI2POS.x", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "THI2POS.z", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "THI2POS.y", 0f);
			thi2PosL = new Vector3(dy / 1000f, dz / 1000f, -dx / 1000f);
			thi2PosR = new Vector3(dy / 1000f, dz / 1000f, dx / 1000f);
		}
		bonePosition["Bip01 L Thigh_SCL_"] = thi2PosL;
		bonePosition["Bip01 R Thigh_SCL_"] = thi2PosR;

		// 元々足の位置と連動しており、追加するときに整合性を保つため足の位置との和で計算
		Vector3 hipPosL;
		Vector3 hipPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "HIPPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "HIPPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "HIPPOS.z", 0f);
			hipPosL = new Vector3(dy / 1000f, dz / 1000f, -dx / 1000f);
			hipPosR = new Vector3(dy / 1000f, dz / 1000f, dx / 1000f);
		}
		bonePosition["Hip_L"] = thiPosL + hipPosL;
		bonePosition["Hip_R"] = thiPosR + hipPosR;

		Vector3 mtwPosL;
		Vector3 mtwPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "MTWPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "MTWPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "MTWPOS.z", 0f);
			mtwPosL = new Vector3(dx / 10f, dy / 10f, dz / 10f);
			mtwPosR = new Vector3(dx / 10f, dy / 10f, -dz / 10f);
		}
		bonePosition["momotwist_L"] = mtwPosL;
		bonePosition["momotwist_R"] = mtwPosR;

		Vector3 mmnPosL;
		Vector3 mmnPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "MMNPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "MMNPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "MMNPOS.z", 0f);
			mmnPosL = new Vector3(dx / 10f, dy / 10f, dz / 10f);
			mmnPosR = new Vector3(dx / 10f, -dy / 10f, dz / 10f);
		}
		bonePosition["momoniku_L"] = mmnPosL;
		bonePosition["momoniku_R"] = mmnPosR;

		Vector3 skirtPos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "SKTPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "SKTPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "SKTPOS.z", 0f);
			skirtPos = new Vector3(-dz / 10f, -dy / 10f, dx / 10f);
		}
		bonePosition["Skirt"] = skirtPos;

		Vector3 spinePos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "SPIPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "SPIPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "SPIPOS.z", 0f);
			spinePos = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
		}
		bonePosition["Bip01 Spine"] = spinePos;

		Vector3 spine0aPos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "S0APOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "S0APOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "S0APOS.z", 0f);
			spine0aPos = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
		}
		bonePosition["Bip01 Spine0a"] = spine0aPos;

		Vector3 spine1Pos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "S1POS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "S1POS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "S1POS.z", 0f);
			spine1Pos = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
		}
		bonePosition["Bip01 Spine1"] = spine1Pos;

		Vector3 spine1aPos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "S1APOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "S1APOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "S1APOS.z", 0f);
			spine1aPos = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
		}
		bonePosition["Bip01 Spine1a"] = spine1aPos;

		Vector3 neckPos;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "NECKPOS.x", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "NECKPOS.y", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "NECKPOS.z", 0f);
			neckPos = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
		}
		bonePosition["Bip01 Neck"] = neckPos;

		Vector3 clvPosL;
		Vector3 clvPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "CLVPOS.x", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "CLVPOS.z", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "CLVPOS.y", 0f);
			clvPosL = new Vector3(-dx / 10f, dy / 10f, dz / 10f);
			clvPosR = new Vector3(-dx / 10f, dy / 10f, -dz / 10f);
		}
		bonePosition["Bip01 L Clavicle"] = clvPosL;
		bonePosition["Bip01 R Clavicle"] = clvPosR;

		Vector3 muneSubPosL;
		Vector3 muneSubPosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "MUNESUBPOS.x", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "MUNESUBPOS.z", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "MUNESUBPOS.y", 0f);
			muneSubPosL = new Vector3(-dy / 10f, dz / 10f, -dx / 10f);
			muneSubPosR = new Vector3(-dy / 10f, -dz / 10f, -dx / 10f);
		}
		bonePosition["Mune_L_sub"] = muneSubPosL;
		bonePosition["Mune_R_sub"] = muneSubPosR;

		Vector3 munePosL;
		Vector3 munePosR;
		{
			float dx = ExSaveData.GetFloat(maid, PluginName, "MUNEPOS.x", 0f);
			float dz = ExSaveData.GetFloat(maid, PluginName, "MUNEPOS.z", 0f);
			float dy = ExSaveData.GetFloat(maid, PluginName, "MUNEPOS.y", 0f);
			munePosL = new Vector3(dz / 10f, -dy / 10f, dx / 10f);
			munePosR = new Vector3(dz / 10f, -dy / 10f, -dx / 10f);
		}
		bonePosition["Mune_L"] = munePosL;
		bonePosition["Mune_R"] = munePosR;

		SetBoneScaleFromList2(boneScale, maid, "CLVSCL", BodyBone.ClavicleScales);
		SetBoneScaleFromList2(boneScale, maid, "UPARMSCL", BodyBone.UpperArmScales);
		SetBoneScaleFromList2(boneScale, maid, "FARMSCL", BodyBone.ForeArmScales);
		SetBoneScaleFromList2(boneScale, maid, "HANDSCL", BodyBone.HandScales);

		SetBoneScaleFromList2(bonePositionRate, maid, "CLVSCL", BodyBone.ClaviclePositions);
		SetBoneScaleFromList2(bonePositionRate, maid, "UPARMSCL", BodyBone.UpperArmPositions);
		SetBoneScaleFromList2(bonePositionRate, maid, "FARMSCL", BodyBone.ForeArmPositions);
		SetBoneScaleFromList2(bonePositionRate, maid, "HANDSCL", BodyBone.HandPositions);

		// スケール変更するボーンをリストに一括登録
		SetBoneScaleFromList(boneScale, maid, BoneAndPropNameList);

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

		float sliderScale = 20f;
		for (int i = boneMorph_.bones.Count - 1; i >= 0; i--) {
			BoneMorphLocal boneMorphLocal = boneMorph_.bones[i];
			Vector3 scl = new Vector3(1f, 1f, 1f);
			Vector3 pos = boneMorphLocal.pos;
			for (int j = 0; j < BoneMorph.PropNames.Length; j++) {
				float s = 1f;
				switch (j) {
					case 0:
						s = boneMorph_.SCALE_Kubi;
						break;
					case 1:
						s = boneMorph_.SCALE_Ude;
						break;
					case 2:
						s = boneMorph_.SCALE_EyeX;
						break;
					case 3:
						s = boneMorph_.SCALE_EyeY;
						break;
					case 4:
						s = boneMorph_.Postion_EyeX * (0.5f + boneMorph_.Postion_EyeY * 0.5f);
						break;
					case 5:
						s = boneMorph_.Postion_EyeY;
						break;
					case 6:
						s = boneMorph_.SCALE_HeadX;
						break;
					case 7:
						s = boneMorph_.SCALE_HeadY;
						break;
					case 8:
						s = boneMorph_.SCALE_DouPer;
						if (boneMorphLocal.Kahanshin == 0f) {
							s = 1f - s;
						}
						break;
					case 9:
						s = boneMorph_.SCALE_Sintyou;
						break;
					case 10:
						s = boneMorph_.SCALE_Koshi;
						break;
					case 11:
						s = boneMorph_.SCALE_Kata;
						break;
					case 12:
						s = boneMorph_.SCALE_West;
						break;
					default:
						s = 1f;
						break;
				}

				if ((boneMorphLocal.atr & 1L << (j & 63)) != 0L) {
					Vector3 v0 = boneMorphLocal.vecs_min[j];
					Vector3 v1 = boneMorphLocal.vecs_max[j];

					Vector3 n0 = v0 * sliderScale - v1 * (sliderScale - 1f);
					Vector3 n1 = v1 * sliderScale - v0 * (sliderScale - 1f);
					float f = (s + sliderScale - 1f) * (1f / (sliderScale * 2.0f - 1f));
					scl = Vector3.Scale(scl, Vector3.Lerp(n0, n1, f));
				}

				if ((boneMorphLocal.atr & 1L << (j + 32 & 63)) != 0L) {
					Vector3 v0 = boneMorphLocal.vecs_min[j + 32];
					Vector3 v1 = boneMorphLocal.vecs_max[j + 32];

					Vector3 n0 = v0 * sliderScale - v1 * (sliderScale - 1f);
					Vector3 n1 = v1 * sliderScale - v0 * (sliderScale - 1f);
					float f = (s + sliderScale - 1f) * (1f / (sliderScale * 2.0f - 1f));
					pos = Vector3.Scale(pos, Vector3.Lerp(n0, n1, f));
				}
			}

			Transform linkT = boneMorphLocal.linkT;
			if (linkT == null) {
				continue;
			}

			string name = linkT.name;

			if (name != null && name.Contains("Thigh_SCL_")) {
				boneMorph_.SnityouOutScale = Mathf.Pow(scl.x, 0.9f);
			}

			// リストに登録されているボーンのスケール設定
			if (name != null && boneScale.ContainsKey(name)) {
				scl = Vector3.Scale(scl, boneScale[name]);
			}

			// リストに登録されているボーンのポジション設定
			if (name != null && bonePosition.ContainsKey(name)) {
				pos += bonePosition[name];
			}

			// リストに登録されているボーンのポジション設定
			if (name != null && bonePositionRate.ContainsKey(name)) {
				pos = Vector3.Scale(pos, bonePositionRate[name]);
			}

			Transform muneLParent = tbody.m_trHitParentL;
			Transform muneLChild = tbody.m_trHitChildL;
			Transform muneRParent = tbody.m_trHitParentR;
			Transform muneRChild = tbody.m_trHitChildR;
			Transform muneLSub = tbody.m_trsMuneLsub;
			Transform muneRSub = tbody.m_trsMuneRsub;
			if (muneLChild && muneLParent && muneRChild && muneRParent) {
				muneLChild.localPosition = muneLSub.localPosition;
				muneLParent.localPosition = muneLSub.localPosition;
				muneRChild.localPosition = muneRSub.localPosition;
				muneRParent.localPosition = muneRSub.localPosition;
			}

			// ignoreHeadBonesに登録されている場合はヒラエルキーを辿って頭のツリーを無視
			if (name != null) {
				if (!(ignoreHeadBones.Contains(name) && CMT.SearchObjObj(maid.body0.m_Bones.transform.Find("Bip01"), linkT))) {
					linkT.localScale = scl;
				}
				linkT.localPosition = pos;
			}

			if (name != null) {
				if (name == "Eyepos_L") {
					tEyePosL = linkT;
				} else if (name == "Eyepos_R") {
					tEyePosR = linkT;
				}
			}
		}

		// 目のサイズ・角度変更
		// EyeScaleRotate : 目のサイズと角度変更する CM3D.MaidVoicePich.Plugin.cs の追加メソッド
		// http://pastebin.com/DBuN5Sws
		// その１>>923
		// http://jbbs.shitaraba.net/bbs/read.cgi/game/55179/1438196715/923
		if (tEyePosL != null) {
			Transform linkT = tEyePosL;
			Vector3 localCenter = linkT.localPosition + (new Vector3(0f, eyeAngY, eyeAngX)); // ローカル座標系での回転中心位置
			Vector3 worldCenter = linkT.parent.TransformPoint(localCenter);         // ワールド座標系での回転中心位置
			Vector3 localAxis = new Vector3(-1f, 0f, 0f);                       // ローカル座標系での回転軸
			Vector3 worldAxis = linkT.TransformDirection(localAxis);               // ワールド座標系での回転軸

			linkT.localRotation = new Quaternion(-0.00560432f, -0.001345155f, 0.06805823f, 0.9976647f);    // 初期の回転量
			linkT.RotateAround(worldCenter, worldAxis, eyeAngAngle);
		}
		if (tEyePosR != null) {
			Transform linkT = tEyePosR;
			Vector3 localCenter = linkT.localPosition + (new Vector3(0f, eyeAngY, -eyeAngX));    // ローカル座標系での回転中心位置
			Vector3 worldCenter = linkT.parent.TransformPoint(localCenter);             // ワールド座標系での回転中心位置
			Vector3 localAxis = new Vector3(-1f, 0f, 0f);                           // ローカル座標系での回転軸
			Vector3 worldAxis = linkT.TransformDirection(localAxis);                   // ワールド座標系での回転軸

			linkT.localRotation = new Quaternion(0.9976647f, 0.06805764f, -0.001350592f, -0.005603582f);   // 初期の回転量
			linkT.RotateAround(worldCenter, worldAxis, -eyeAngAngle);
		}

		// COM3D2追加処理
		// ボーンポジション系
		foreach (var o in boneMorph_.m_listBoneMorphPos) {
			string strPropName = o.strPropName;
			Transform trs = o.trBone;
			Vector3 defPos = o.m_vDefPos;
			Vector3 addMin = o.m_vAddMin;
			Vector3 addMax = o.m_vAddMax;

			if (strPropName == "Nosepos")
				trs.localPosition = Lerp(addMin, defPos, addMax, (float)boneMorph_.POS_Nose, sliderScale);
			else if (strPropName == "MayuY") {
				trs.localPosition = Lerp(addMin, defPos, addMax, (float)boneMorph_.POS_MayuY, sliderScale);
			} else if (strPropName == "EyeBallPosYL" || strPropName == "EyeBallPosYR") {
				trs.localPosition = Lerp(addMin, defPos, addMax, (float)boneMorph_.EyeBallPosY, sliderScale);
			} else if (strPropName == "Mayupos_L" || strPropName == "Mayupos_R") {
				Vector3 vector3_1 = Lerp(addMin, defPos, addMax, (float)boneMorph_.POS_MayuY, sliderScale);
				float x1 = addMin.x;
				addMin.x = addMax.x;
				addMax.x = x1;
				Vector3 vector3_2 = Lerp(addMin, defPos, addMax, (float)boneMorph_.POS_MayuX, sliderScale);
				float x3 = vector3_2.x + vector3_1.x - defPos.x;
				trs.localPosition = new Vector3(x3, vector3_1.y, vector3_2.z);
			}
		}

		// ボーンスケール系
		foreach (var o in boneMorph_.m_listBoneMorphScl) {
			string strPropName = o.strPropName;
			Transform trs = o.trBone;
			Vector3 defScl = o.m_vDefScl;
			Vector3 addMin = o.m_vAddMin;
			Vector3 addMax = o.m_vAddMax;

			if (strPropName == "Earscl_L" || strPropName == "Earscl_R") {
				trs.localScale = Lerp(addMin, defScl, addMax, (float)boneMorph_.SCALE_Ear, sliderScale);
			} else if (strPropName == "Nosescl") {
				trs.localScale = Lerp(addMin, defScl, addMax, (float)boneMorph_.SCALE_Nose, sliderScale);
			} else if (strPropName == "EyeBallSclXL" || strPropName == "EyeBallSclXR") {
				Vector3 localScale = trs.localScale;
				localScale.z = Lerp(addMin, defScl, addMax, (float)boneMorph_.EyeBallSclX, sliderScale).z;
				trs.localScale = localScale;
			} else if (strPropName == "EyeBallSclYL" || strPropName == "EyeBallSclYR") {
				Vector3 localScale = trs.localScale;
				localScale.y = Lerp(addMin, defScl, addMax, (float)boneMorph_.EyeBallSclY, sliderScale).y;
				trs.localScale = localScale;
			}
		}

		// ボーンローテーション系
		foreach (var o in boneMorph_.m_listBoneMorphRot) {
			string strPropName = o.strPropName;
			Transform trs = o.trBone;
			Quaternion defRot = o.m_vDefRotate;
			Quaternion addMin = o.m_vAddMin;
			Quaternion addMax = o.m_vAddMax;

			if (strPropName == "Earrot_L" || strPropName == "Earrot_R") {
				trs.localRotation = RotLerp(addMin, defRot, addMax, (float)boneMorph_.ROT_Ear, sliderScale);
			} else if (strPropName == "Mayurot_L" || strPropName == "Mayurot_R") {
				trs.localRotation = RotLerp(addMin, defRot, addMax, (float)boneMorph_.ROT_Mayu, sliderScale);
			}
		}
	}

	public static Vector3 Lerp(Vector3 min, Vector3 def, Vector3 max, float t, float sliderScale) {
		if ((double)t >= 0.5) {
			Vector3 n1 = max + (max - def) * (sliderScale - 1f) * 2;
			float f = (t - 0.5f) * (1f / (sliderScale * 2.0f - 1f)) * 2.0f;
			return Vector3.Lerp(def, n1, f);
		} else {
			Vector3 n0 = min - (def - min) * (sliderScale - 1f) * 2;
			float f = (t + sliderScale - 1f) * (1f / (sliderScale * 2.0f - 1f)) * 2.0f;
			return Vector3.Lerp(n0, def, f);
		}
	}

	public static Quaternion RotLerp(Quaternion min, Quaternion def, Quaternion max, float t, float sliderScale) {
		float t1 = (double)t > 0.5 ? (float)(((double)t - 0.5) / 0.5) : t / 0.5f;
		if ((double)t <= 0.5)
			return Quaternion.LerpUnclamped(min, def, t1);
		return Quaternion.LerpUnclamped(def, max, t1);

	}

	private Vector3 GetBoneScale(Dictionary<string, Vector3> boneScaleDic, string boneName) {
		if (boneScaleDic.ContainsKey(boneName)) {
			return boneScaleDic[boneName];
		} else {
			return Vector3.one;
		}
	}

	private void SetBoneScaleFromList(Dictionary<string, Vector3> dictionary, Maid maid, KeyValuePair<string, string>[] boneAndPropNameList) {
		foreach (var item in boneAndPropNameList) {
			var boneName = item.Key;
			if (boneName.Contains("?")) {
				string boneNameL = boneName.Replace('?', 'L');
				string boneNameR = boneName.Replace('?', 'R');
				SetBoneScale(dictionary, boneNameL, maid, item.Value);
				dictionary[boneNameR] = dictionary[boneNameL];
			} else {
				SetBoneScale(dictionary, boneName, maid, item.Value);
			}
		}
	}

	private void SetBoneScaleFromList2(Dictionary<string, Vector3> dictionary, Maid maid, string tag, List<string> boneList) {
		foreach (string boneName in boneList) {
			if (boneName.Contains("?")) {
				string boneNameL = boneName.Replace('?', 'L');
				string boneNameR = boneName.Replace('?', 'R');
				SetBoneScale(dictionary, boneNameL, maid, tag);
				dictionary[boneNameR] = dictionary[boneNameL];
			} else {
				SetBoneScale(dictionary, boneName, maid, tag);
			}
		}
	}

	private void SetBoneScale(Dictionary<string, Vector3> dictionary, string boneName, Maid maid, string propName) {
		dictionary[boneName] = Vector3.Scale(GetBoneScale(dictionary, boneName),
			new Vector3(
				ExSaveData.GetFloat(maid, PluginName, propName + ".height", 1f),
				ExSaveData.GetFloat(maid, PluginName, propName + ".depth", 1f),
				ExSaveData.GetFloat(maid, PluginName, propName + ".width", 1f)));
	}

	private static string getHiraerchy(Transform t) {
		if (!t) {
			return string.Empty;
		}
		string hiraerchy = "/" + t.name;
		while (t.parent) {
			t = t.parent;
			hiraerchy = "/" + t.name + hiraerchy;
		}

		return hiraerchy;
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
	}
}
