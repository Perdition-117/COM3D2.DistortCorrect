using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using CM3D2.AddModsSlider.Plugin;
using CM3D2.ExternalSaveData.Managed;

namespace COM3D2.DistortCorrect.Plugin {
	[PluginName("DistortCorrect")]
	[PluginVersion("0.4.0.5")]
	class DistortCorrectPlugin : PluginBase {
		BoneMorph bm = new BoneMorph();
		bool sceneChange = false;
		CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch maidVoicePitch;

		Dictionary<Maid, bool> limbFixDic = new Dictionary<Maid, bool>();

		void Start() {
			CM3D2.AddModsSlider.Plugin.AddModsSlider.ExternalModsParam emp =
				new CM3D2.AddModsSlider.Plugin.AddModsSlider.ExternalModsParam(
					"LIMBSFIX",
					"手足の歪み修正",
					true,
					"toggle",
					"FACE_OFF"
				);
			CM3D2.AddModsSlider.Plugin.AddModsSlider.AddExternalModsParam(emp);

			maidVoicePitch = FindObjectOfType<CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch>();
		}

		void OnLevelWasLoaded(int level) {
			sceneChange = true;
			limbFixDic = new Dictionary<Maid, bool>();
		}

		void Update() {
			if (sceneChange) {
				// BoneMorph_.Blend 処理終了後のコールバック
				CM3D2.MaidVoicePitch.Managed.Callbacks.BoneMorph_.Blend.Callbacks["CM3D2.MaidVoicePitch"] = boneMorph_BlendCallback;
				sceneChange = false;
			}
		}

		/// <summary>
		/// BoneMorph_.Blend の処理終了後に呼ばれるコールバック。
		/// 初期化、設定変更時のみ呼び出される。
		/// ボーンのブレンド処理が行われる際、拡張スライダーに関連する補正は基本的にここで行う。
		/// 毎フレーム呼び出されるわけではないことに注意
		/// </summary>
		MethodInfo mi_WideSlider = typeof(CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch).GetMethod("WideSlider", BindingFlags.NonPublic | BindingFlags.Instance);
		MethodInfo mi_EyeBall = typeof(CM3D2.MaidVoicePitch.Plugin.MaidVoicePitch).GetMethod("EyeBall", BindingFlags.NonPublic | BindingFlags.Instance);
		void boneMorph_BlendCallback(BoneMorph_ boneMorph_) {
			Maid maid = PluginHelper.GetMaid(boneMorph_);
			if (maid == null) {
				return;
			}

			bool wideSlider = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "WIDESLIDER", false);
			bool limbFix = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "LIMBSFIX", false);
			bool enable = wideSlider && limbFix;
			if (enable) {
				WideSlider(maid);
			} else {
				mi_WideSlider.Invoke(maidVoicePitch, new object[] { maid });
			}
			//mi_EyeBall.Invoke(maidVoicePitch, new object[] { maid });
			if (SceneManager.GetActiveScene().name != "ScenePhotoMode") {
				if (maid.body0 != null && maid.body0.isLoadedBody) {
					IKPreInit(maid);
					maid.body0.IKCtrl.Init();
				}
			}
		}

		void IKPreInit(Maid maid) {
			FullBodyIKCtrl fbikc = maid.body0.IKCtrl;
			Transform mouth = (Transform)Helper.GetInstanceField(typeof(FullBodyIKCtrl), fbikc, "m_Mouth");
			if (mouth) DestroyImmediate(mouth.gameObject);
			Transform nippleL = (Transform)Helper.GetInstanceField(typeof(FullBodyIKCtrl), fbikc, "m_NippleL");
			if (nippleL) DestroyImmediate(nippleL.gameObject);
			Transform nippleR = (Transform)Helper.GetInstanceField(typeof(FullBodyIKCtrl), fbikc, "m_NippleR");
			if (nippleR) DestroyImmediate(nippleR.gameObject);
		}

		// WideSlider
		string PluginName { get { return "CM3D2.MaidVoicePitch"; } }
		BodyBone bodyBone = new BodyBone();

		private static string[][] boneAndPropNameList = new string[][]
{
				new string[] { "Bip01 ? Thigh_SCL_", "THISCL" },         // 下半身
				new string[] { "Bip01 ? Calf_SCL_", "THISCL" },         // 下半身
				new string[] { "Bip01 ? Foot", "THISCL" },         // 下半身
				new string[] { "momotwist_?", "THISCL" },         // 下半身
				new string[] { "momotwist_?", "MTWSCL" },         // ももツイスト
				new string[] { "momoniku_?", "MMNSCL" },         // もも肉
				new string[] { "Bip01 Pelvis_SCL_", "PELSCL" },     // 骨盤
				new string[] { "Hip_?", "PELSCL" },     // 骨盤
				new string[] { "Hip_?", "HIPSCL" },     // 骨盤
				new string[] { "Bip01 ? Thigh_SCL_", "THISCL2" },   // 膝
				new string[] { "Bip01 ? Calf_SCL_", "CALFSCL" },         // 膝下
				new string[] { "Bip01 ? Foot", "CALFSCL" },         // 膝下
				new string[] { "Bip01 ? Foot", "FOOTSCL" },         // 足首より下
				new string[] { "Skirt", "SKTSCL" },                 // スカート
				new string[] { "Bip01 Spine_SCL_", "SPISCL" },      // 胴(下腹部周辺)
				new string[] { "Bip01 Spine0a_SCL_", "S0ASCL" },    // 胴0a(腹部周辺)
				new string[] { "Bip01 Spine1_SCL_", "S1_SCL" },     // 胴1_(みぞおち周辺)
				new string[] { "Bip01 Spine1a_SCL_", "S1ASCL" },    // 胴1a(首・肋骨周辺)
				new string[] { "Bip01 Spine1a", "S1ABASESCL" },     // 胴1a(胸より上)※頭に影響有り

				new string[] { "Kata_?", "KATASCL" },               // 肩
				new string[] { "Mune_?", "MUNESCL" },               // 胸
				new string[] { "Mune_?_sub", "MUNESUBSCL" },        // 胸サブ
				new string[] { "Bip01 Neck_SCL_", "NECKSCL" },      // 首

   //new string[] { "", "" },
};

		FieldInfo f_muneLParent = Helper.GetFieldInfo(typeof(TBody), "m_trHitParentL");
		FieldInfo f_muneLChild = Helper.GetFieldInfo(typeof(TBody), "m_trHitChildL");
		FieldInfo f_muneRParent = Helper.GetFieldInfo(typeof(TBody), "m_trHitParentR");
		FieldInfo f_muneRChild = Helper.GetFieldInfo(typeof(TBody), "m_trHitChildR");
		FieldInfo f_muneLSub = Helper.GetFieldInfo(typeof(TBody), "m_trsMuneLsub");
		FieldInfo f_muneRSub = Helper.GetFieldInfo(typeof(TBody), "m_trsMuneRsub");

		void WideSlider(Maid maid) {
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

			SetBoneScaleFromList2(boneScale, maid, "CLVSCL", bodyBone.ClavicleScaleList);
			SetBoneScaleFromList2(boneScale, maid, "UPARMSCL", bodyBone.upperArmScaleList);
			SetBoneScaleFromList2(boneScale, maid, "FARMSCL", bodyBone.foreArmScaleList);
			SetBoneScaleFromList2(boneScale, maid, "HANDSCL", bodyBone.handScaleList);

			SetBoneScaleFromList2(bonePositionRate, maid, "CLVSCL", bodyBone.ClaviclePositionList);
			SetBoneScaleFromList2(bonePositionRate, maid, "UPARMSCL", bodyBone.upperArmPositionList);
			SetBoneScaleFromList2(bonePositionRate, maid, "FARMSCL", bodyBone.foreArmPositionList);
			SetBoneScaleFromList2(bonePositionRate, maid, "HANDSCL", bodyBone.handPositionList);

			// スケール変更するボーンをリストに一括登録
			SetBoneScaleFromList(boneScale, maid, boneAndPropNameList);

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

				Transform muneLParent = f_muneLParent.GetValue(tbody) as Transform;
				Transform muneLChild = f_muneLChild.GetValue(tbody) as Transform;
				Transform muneRParent = f_muneRParent.GetValue(tbody) as Transform;
				Transform muneRChild = f_muneRChild.GetValue(tbody) as Transform;
				Transform muneLSub = f_muneLSub.GetValue(tbody) as Transform;
				Transform muneRSub = f_muneRSub.GetValue(tbody) as Transform;
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
			var listBoneMorphPos = Helper.GetInstanceField(typeof(BoneMorph_), boneMorph_, "m_listBoneMorphPos");
			var ienumPos = listBoneMorphPos as IEnumerable;

			System.Reflection.Assembly asmPos = System.Reflection.Assembly.GetAssembly(typeof(BoneMorph_));
			Type listBoneMorphPosType = listBoneMorphPos.GetType();
			Type boneMorphPosType = asmPos.GetType("BoneMorph_+BoneMorphPos");

			foreach (object o in ienumPos) {
				string strPropName = (string)Helper.GetInstanceField(boneMorphPosType, o, "strPropName");
				Transform trs = (Transform)Helper.GetInstanceField(boneMorphPosType, o, "trBone");
				Vector3 defPos = (Vector3)Helper.GetInstanceField(boneMorphPosType, o, "m_vDefPos");
				Vector3 addMin = (Vector3)Helper.GetInstanceField(boneMorphPosType, o, "m_vAddMin");
				Vector3 addMax = (Vector3)Helper.GetInstanceField(boneMorphPosType, o, "m_vAddMax");

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
			var listBoneMorphScl = Helper.GetInstanceField(typeof(BoneMorph_), boneMorph_, "m_listBoneMorphScl");
			var ienumScl = listBoneMorphScl as IEnumerable;

			System.Reflection.Assembly asmScl = System.Reflection.Assembly.GetAssembly(typeof(BoneMorph_));
			Type listBoneMorphSclType = listBoneMorphScl.GetType();
			Type boneMorphSclType = asmScl.GetType("BoneMorph_+BoneMorphScl");

			foreach (object o in ienumScl) {
				string strPropName = (string)Helper.GetInstanceField(boneMorphSclType, o, "strPropName");
				Transform trs = (Transform)Helper.GetInstanceField(boneMorphSclType, o, "trBone");
				Vector3 defScl = (Vector3)Helper.GetInstanceField(boneMorphSclType, o, "m_vDefScl");
				Vector3 addMin = (Vector3)Helper.GetInstanceField(boneMorphSclType, o, "m_vAddMin");
				Vector3 addMax = (Vector3)Helper.GetInstanceField(boneMorphSclType, o, "m_vAddMax");

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
			var listBoneMorphRot = Helper.GetInstanceField(typeof(BoneMorph_), boneMorph_, "m_listBoneMorphRot");
			var ienumRot = listBoneMorphRot as IEnumerable;

			System.Reflection.Assembly asmRot = System.Reflection.Assembly.GetAssembly(typeof(BoneMorph_));
			Type listBoneMorphRotType = listBoneMorphRot.GetType();
			Type boneMorphRotType = asmRot.GetType("BoneMorph_+BoneMorphRotatio");

			foreach (object o in ienumRot) {
				string strPropName = (string)Helper.GetInstanceField(boneMorphRotType, o, "strPropName");
				Transform trs = (Transform)Helper.GetInstanceField(boneMorphRotType, o, "trBone");
				Quaternion defRot = (Quaternion)Helper.GetInstanceField(boneMorphRotType, o, "m_vDefRotate");
				Quaternion addMin = (Quaternion)Helper.GetInstanceField(boneMorphRotType, o, "m_vAddMin");
				Quaternion addMax = (Quaternion)Helper.GetInstanceField(boneMorphRotType, o, "m_vAddMax");

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

		Vector3 GetBoneScale(Dictionary<string, Vector3> boneScaleDic, string boneName) {
			if (boneScaleDic.ContainsKey(boneName)) {
				return boneScaleDic[boneName];
			} else {
				return Vector3.one;
			}
		}

		void SetBoneScaleFromList(Dictionary<string, Vector3> dictionary, Maid maid, string[][] _boneAndPropNameList) {
			foreach (var item in _boneAndPropNameList) {
				if (item[0].Contains("?")) {
					string boneNameL = item[0].Replace('?', 'L');
					string boneNameR = item[0].Replace('?', 'R');
					SetBoneScale(dictionary, boneNameL, maid, item[1]);
					dictionary[boneNameR] = dictionary[boneNameL];
				} else {
					SetBoneScale(dictionary, item[0], maid, item[1]);
				}
			}
		}

		void SetBoneScaleFromList2(Dictionary<string, Vector3> dictionary, Maid maid, string tag, List<string> boneList) {
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

		void SetBoneScale(Dictionary<string, Vector3> dictionary, string boneName, Maid maid, string propName) {
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
	}
}
