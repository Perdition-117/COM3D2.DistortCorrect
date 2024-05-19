using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace COM3D2.DistortCorrect.Patcher {
	public static class Patcher {
		public static readonly string[] TargetAssemblyNames = { "Assembly-CSharp.dll" };

		public static void Patch(AssemblyDefinition assembly) {
			AssemblyDefinition ta = assembly;
			AssemblyDefinition da = PatcherHelper.GetAssemblyDefinition("COM3D2.DistortCorrect.Managed.dll");
			TypeDefinition typedef = da.MainModule.GetType("CM3D2.DistortCorrect.Managed.DistortCorrectManaged");


			{
				// 引数の型
				string[] targetArgTypes = {
					"MPN",
					"System.String",
					"System.String",
					"System.String",
					"System.String",
					"System.Boolean",
					"System.Int32",
				};

				string[] calleeArgTypes = {
					"TBody",
					"MPN",
					"System.String",
					"System.String",
					"System.String",
					"System.String",
					"System.Boolean",
					"System.Int32",
				};

				PatcherHelper.SetHook(
					PatcherHelper.HookType.PreCall,
					ta, "TBody.AddItem", targetArgTypes,
					da, "CM3D2.DistortCorrect.Managed.DistortCorrectManaged.AddItem", calleeArgTypes);
			}

			PatcherHelper.SetHook(
				PatcherHelper.HookType.PreCall,
				ta, "TBody.DelItem",
				da, "CM3D2.DistortCorrect.Managed.DistortCorrectManaged.DelItem");

			PatcherHelper.SetHook(
				PatcherHelper.HookType.PreCall,
				ta, "BoneMorph_.Blend",
				da, "CM3D2.DistortCorrect.Managed.DistortCorrectManaged.PreBlend");

			MethodDefinition cc1 = ta.MainModule.GetType("ImportCM").Methods.First(m => m.Name == "LoadSkinMesh_R");
			HookLoadMesh(da, typedef, cc1);
			MethodDefinition cc2 = ta.MainModule.GetType("ImportCM").Methods.First(m => m.Name == "LoadOnlyBone_R");
			HookLoadBone(da, typedef, cc2);
		}

		static void HookLoadMesh(AssemblyDefinition da, TypeDefinition typedef, MethodDefinition cc) {
			Console.WriteLine("method " + cc.Name);
			var it = cc.Body.Instructions.First(i => (i.OpCode == OpCodes.Ldarg_0));

			// string name = r.ReadByte();まで移動
			for (int i = cc.Body.Instructions.IndexOf(it); i < cc.Body.Instructions.Count(); i++) // 無限ループ防止
			{

				if (it.OpCode == OpCodes.Callvirt && ((MethodReference)it.Operand).ToString().Contains("BinaryReader::ReadByte")) {
					for (int index = 0; index < 6; index++) {
						it = it.Next;
					}

					Console.WriteLine("it " + it);
					Console.WriteLine("next " + it.Next);

					if (it.OpCode == OpCodes.Ldloc_S && it.Next.OpCode == OpCodes.Brfalse) {
						// string text2 = r.ReadString();
						it = it.Next;
						ILProcessor il = cc.Body.GetILProcessor();
						il.InsertBefore(it, il.Create(OpCodes.Ldloc, 15));
						il.InsertBefore(it, il.Create(OpCodes.Call, cc.Module.ImportReference(PatcherHelper.GetMethod(typedef, "JudgeSclBone"))));
						break;
					}
				}
				it = it.Next;
			}
		}

		static void HookLoadBone(AssemblyDefinition da, TypeDefinition typedef, MethodDefinition cc) {
			Console.WriteLine("method " + cc.Name);
			var it = cc.Body.Instructions.First(i => (i.OpCode == OpCodes.Ldarg_0));

			// string name = r.ReadByte();まで移動
			for (int i = cc.Body.Instructions.IndexOf(it); i < cc.Body.Instructions.Count(); i++) // 無限ループ防止
			{

				if (it.OpCode == OpCodes.Callvirt && ((MethodReference)it.Operand).ToString().Contains("BinaryReader::ReadByte")) {
					for (int index = 0; index < 6; index++) {
						it = it.Next;
					}

					Console.WriteLine("it " + it);
					Console.WriteLine("next " + it.Next);

					if (it.OpCode == OpCodes.Ldloc_S && it.Next.OpCode == OpCodes.Brfalse) {
						// string text2 = r.ReadString();
						it = it.Next;
						ILProcessor il = cc.Body.GetILProcessor();
						il.InsertBefore(it, il.Create(OpCodes.Ldloc, 10));
						il.InsertBefore(it, il.Create(OpCodes.Call, cc.Module.ImportReference(PatcherHelper.GetMethod(typedef, "JudgeSclBone"))));
						break;
					}
				}
				it = it.Next;
			}
		}
	}
}