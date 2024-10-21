using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CM3D2.ExternalSaveData.Managed;

namespace CM3D2.DistortCorrect.Managed;

public class DistortCorrectManaged {
	private static readonly HashSet<string> ScaleBoneHash = new() {
		"Bip01 L UpperArm",
		"Bip01 L Forearm",
		"Bip01 L Hand",
		"Bip01 L Finger0",
		"Bip01 L Finger01",
		"Bip01 L Finger02",
		"Bip01 L Finger1",
		"Bip01 L Finger11",
		"Bip01 L Finger12",
		"Bip01 L Finger2",
		"Bip01 L Finger21",
		"Bip01 L Finger22",
		"Bip01 L Finger3",
		"Bip01 L Finger31",
		"Bip01 L Finger32",
		"Bip01 L Finger4",
		"Bip01 L Finger41",
		"Bip01 L Finger42",
		"Bip01 L Calf",
		"Bip01 R UpperArm",
		"Bip01 R Forearm",
		"Bip01 R Hand",
		"Bip01 R Finger0",
		"Bip01 R Finger01",
		"Bip01 R Finger02",
		"Bip01 R Finger1",
		"Bip01 R Finger11",
		"Bip01 R Finger12",
		"Bip01 R Finger2",
		"Bip01 R Finger21",
		"Bip01 R Finger22",
		"Bip01 R Finger3",
		"Bip01 R Finger31",
		"Bip01 R Finger32",
		"Bip01 R Finger4",
		"Bip01 R Finger41",
		"Bip01 R Finger42",
		"Bip01 R Calf",
	};

	private static readonly Dictionary<Maid, bool> LimbFixes = new();

	private static Dictionary<string, Vector3> OriginalBones;
	private static Dictionary<string, List<BoneMorph.BoneProp>> OriginalBones2;
	private static Dictionary<string, Vector3> NewBones;
	private static Dictionary<string, List<BoneMorph.BoneProp>> NewBones2;

	public static void AddItem(TBody tBody, MPN mpn, string slotname, string filename, string AttachSlot, string AttachName, bool f_bTemp, int version) {
		ResetBoneDic(tBody.maid, true);
	}

	public static void DelItem(TBody tBody, MPN mpn, string slotname) {
		ResetBoneDic(tBody.maid, true);
	}

	public static void PreBlend(BoneMorph_ boneMorph) {
		TryGetMaid(boneMorph, out var maid);
		ResetBoneDic(maid, false);
	}

	public static void ResetBoneDic(Maid maid, bool staticFlag) {
		if (OriginalBones == null) {
			InitBoneDic();
		}

		if (maid == null || maid.IsCrcBody) {
			return;
		}

		var wideSlider = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "WIDESLIDER", false);
		var limbFix = ExSaveData.GetBool(maid, "CM3D2.MaidVoicePitch", "LIMBSFIX", false);
		var enable = wideSlider && limbFix;
		if (staticFlag || !LimbFixes.ContainsKey(maid) || (LimbFixes[maid] != enable)) {
			if (enable) {
				BoneMorph.dic = NewBones;
				BoneMorph.dic2 = NewBones2;
			} else {
				BoneMorph.dic = OriginalBones;
				BoneMorph.dic2 = OriginalBones2;
			}
			maid.body0.bonemorph.Init();
			maid.body0.bonemorph.AddRoot(maid.body0.m_Bones.transform);
			LimbFixes[maid] = enable;
		}
	}

	private static void InitBoneDic() {
		OriginalBones = BoneMorph.dic;
		OriginalBones2 = BoneMorph.dic2;

		BoneMorph.dic = new();
		BoneMorph.dic2 = new();

		BoneMorph.SetPosition("KubiScl", "Bip01 Neck", 0.95f, 1f, 1f, 1.05f, 1f, 1f);
		BoneMorph.SetPosition("KubiScl", "Bip01 Head", 0.8f, 1f, 1f, 1.2f, 1f, 1f);

		SetUdeScale("UdeScl", 0.85f, 1f, 1f, 1.15f, 1f, 1f);

		BoneMorph.SetScale("EyeSclX", "Eyepos_L", 1f, 1f, 0.92f, 1f, 1f, 1.08f);
		BoneMorph.SetScale("EyeSclX", "Eyepos_R", 1f, 1f, 0.92f, 1f, 1f, 1.08f);
		BoneMorph.SetScale("EyeSclY", "Eyepos_L", 1f, 0.92f, 1f, 1f, 1.08f, 1f);
		BoneMorph.SetScale("EyeSclY", "Eyepos_R", 1f, 0.92f, 1f, 1f, 1.08f, 1f);
		BoneMorph.SetPosition("EyePosX", "Eyepos_R", 1f, 1f, 0.9f, 1f, 1f, 1.1f);
		BoneMorph.SetPosition("EyePosX", "Eyepos_L", 1f, 1f, 0.9f, 1f, 1f, 1.1f);
		BoneMorph.SetPosition("EyePosY", "Eyepos_R", 1f, 0.93f, 1f, 1f, 1.07f, 1f);
		BoneMorph.SetPosition("EyePosY", "Eyepos_L", 1f, 0.93f, 1f, 1f, 1.07f, 1f);
		BoneMorph.SetScale("HeadX", "Bip01 Head", 1f, 0.9f, 0.8f, 1f, 1.1f, 1.2f);
		BoneMorph.SetScale("HeadY", "Bip01 Head", 0.8f, 0.9f, 1f, 1.2f, 1.1f, 1f);

		BoneMorph.SetPosition("DouPer", "Bip01 Spine", 1f, 1f, 0.94f, 1f, 1f, 1.06f);
		BoneMorph.SetPosition("DouPer", "Bip01 Spine0a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("DouPer", "Bip01 Spine1", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("DouPer", "Bip01 Spine1a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("DouPer", "Bip01 Neck", 1.03f, 1f, 1f, 0.97f, 1f, 1f);
		BoneMorph.SetPosition("DouPer", "Bip01 ? Calf", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetPosition("DouPer", "Bip01 ? Foot", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("DouPer", "Bip01 ? Thigh_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("DouPer", "momotwist_?", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("DouPer", "Bip01 ? Calf_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		SetUdeScale("DouPer", 0.98f, 1f, 1f, 1.02f, 1f, 1f);

		BoneMorph.SetPosition("sintyou", "Bip01 Spine", 1f, 1f, 0.85f, 1f, 1f, 1.15f);
		BoneMorph.SetPosition("sintyou", "Bip01 Spine0a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 Spine1", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 Spine1a", 0.88f, 1f, 1f, 1.12f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 Neck", 0.97f, 1f, 1f, 1.03f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 Head", 0.9f, 1f, 1f, 1.1f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 ? Calf", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetPosition("sintyou", "Bip01 ? Foot", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Thigh_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "momotwist_?", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Calf_SCL_", 0.87f, 1f, 1f, 1.13f, 1f, 1f);
		SetUdeScale("sintyou", 0.9f, 1f, 1f, 1.1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Thigh", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "momoniku_?", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Pelvis_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Calf", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Foot", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Skirt", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Spine_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Spine0a_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Spine1_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Spine1a_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Spine1a", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Clavicle", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Clavicle_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? UpperArm", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Forearm", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 ? Hand", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Kata_?", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Mune_?", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Mune_?_sub", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("sintyou", "Bip01 Neck_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);


		BoneMorph.SetScale("koshi", "Bip01 Pelvis_SCL_", 1f, 0.8f, 0.92f, 1f, 1.2f, 1.08f);
		BoneMorph.SetScale("koshi", "Bip01 Spine_SCL_", 1f, 1f, 1f, 1f, 1f, 1f);
		BoneMorph.SetScale("koshi", "Hip_?", 1f, 0.96f, 0.9f, 1f, 1.04f, 1.1f);
		BoneMorph.SetScale("koshi", "Skirt", 1f, 0.85f, 0.88f, 1f, 1.2f, 1.12f);
		BoneMorph.SetPosition("kata", "Bip01 ? Clavicle", 0.98f, 1f, 0.5f, 1.02f, 1f, 1.5f);
		BoneMorph.SetScale("kata", "Bip01 Spine1a_SCL_", 1f, 1f, 0.95f, 1f, 1f, 1.05f);
		BoneMorph.SetScale("west", "Bip01 Spine_SCL_", 1f, 0.95f, 0.9f, 1f, 1.05f, 1.1f);
		BoneMorph.SetScale("west", "Bip01 Spine0a_SCL_", 1f, 0.85f, 0.7f, 1f, 1.15f, 1.3f);
		BoneMorph.SetScale("west", "Bip01 Spine1_SCL_", 1f, 0.9f, 0.85f, 1f, 1.1f, 1.15f);
		BoneMorph.SetScale("west", "Bip01 Spine1a_SCL_", 1f, 0.95f, 0.95f, 1f, 1.05f, 1.05f);
		BoneMorph.SetScale("west", "Skirt", 1f, 0.92f, 0.88f, 1f, 1.08f, 1.12f);

		NewBones = BoneMorph.dic;
		NewBones2 = BoneMorph.dic2;

		foreach (var str in NewBones.Keys.Where(e => !OriginalBones.ContainsKey(e))) {
			OriginalBones.Add(str, Vector3.one);
		}

		foreach (var kvp in NewBones2) {
			if (!OriginalBones2.ContainsKey(kvp.Key)) {
				OriginalBones2.Add(kvp.Key, new());
			}
			var propList = OriginalBones2[kvp.Key];
			foreach (var prop in kvp.Value) {
				if (!propList.Exists(e => e.strProp == prop.strProp)) {
					propList.Add(new() {
						strProp = prop.strProp,
						nIndex = prop.nIndex,
						bExistP = prop.bExistP,
						bExistM = prop.bExistM,
						vMinP = Vector3.one,
						vMaxP = Vector3.one,
						vMinM = Vector3.one,
						vMaxM = Vector3.one,
					});
				}
			}
		}
	}

	private static void SetUdeScale(string tag, float x, float y, float z, float x2, float y2, float z2) {
		void SetScale(string boneName) => BoneMorph.SetScale(tag, boneName, x, y, z, x2, y2, z2);
		void SetPosition(string boneName) => BoneMorph.SetPosition(tag, boneName, x, y, z, x2, y2, z2);
		SetScale("Bip01 ? UpperArm_SCL_");
		SetScale("Uppertwist_?");
		SetScale("Uppertwist1_?");
		SetPosition("Uppertwist1_?");
		SetPosition("Bip01 ? Forearm");
		SetScale("Bip01 ? Forearm_SCL_");
		SetScale("Foretwist_?");
		SetScale("Foretwist1_?");
		SetPosition("Foretwist_?");
		SetPosition("Bip01 ? Hand");
		SetScale("Bip01 ? Hand_SCL_");
		SetPosition("Bip01 ? Finger0");
		SetPosition("Bip01 ? Finger1");
		SetPosition("Bip01 ? Finger2");
		SetPosition("Bip01 ? Finger3");
		SetPosition("Bip01 ? Finger4");
		SetScale("Bip01 ? Finger0_SCL_");
		SetPosition("Bip01 ? Finger01");
		SetScale("Bip01 ? Finger01_SCL_");
		SetPosition("Bip01 ? Finger02");
		SetScale("Bip01 ? Finger02_SCL_");
		SetScale("Bip01 ? Finger1_SCL_");
		SetPosition("Bip01 ? Finger11");
		SetScale("Bip01 ? Finger11_SCL_");
		SetPosition("Bip01 ? Finger12");
		SetScale("Bip01 ? Finger12_SCL_");
		SetScale("Bip01 ? Finger2_SCL_");
		SetPosition("Bip01 ? Finger21");
		SetScale("Bip01 ? Finger21_SCL_");
		SetPosition("Bip01 ? Finger22");
		SetScale("Bip01 ? Finger22_SCL_");
		SetScale("Bip01 ? Finger3_SCL_");
		SetPosition("Bip01 ? Finger31");
		SetScale("Bip01 ? Finger31_SCL_");
		SetPosition("Bip01 ? Finger32");
		SetScale("Bip01 ? Finger32_SCL_");
		SetScale("Bip01 ? Finger4_SCL_");
		SetPosition("Bip01 ? Finger41");
		SetScale("Bip01 ? Finger41_SCL_");
		SetPosition("Bip01 ? Finger42");
		SetScale("Bip01 ? Finger42_SCL_");
	}

	public static bool JudgeSclBone(bool flag, GameObject bone) => flag || ScaleBoneHash.Contains(bone.name);

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
