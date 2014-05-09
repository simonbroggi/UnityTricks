using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/**
 * add mesh to bones marked by name.
 * 
 * mesh must be in a separate blend file named xxxUtils.blend where xxx is the name of the referencing blend file.
 * bone must be named Utils_Mesh_nnn where nnn is the name of the linked object in xxxUtils.blend. Bone names with trailing _L _R _l _r .001 .002 .003 will also work
 * The bones must have the deform flag set, otherwise they might not get exported.
 * 
 * When changing names and reimporting check Unitys Mask option if you get errors with the animations and if you are using Mecanim (in Project window under Animations)
 * 
 * known issues:
 * - the bone must have a length of 1 in edit mode for the it to appear scaled correctly.
 * - the objects transforms in the xxxUtils.blend will not be imported correctly. theyre mesh is rotated and theyre transforms aren't. Doesn't really matter does it?!
 * - if the Optimize Game Objects option is used the bones with mesh must be exposed.
 */

public class UtilsImporter : AssetPostprocessor {

	void OnPostprocessModel(GameObject g){
		if(assetPath.EndsWith("Utils.blend")){
			//rotate meshes

			rotateMeshesRec(g);

		}
		else if(assetPath.EndsWith(".blend")){

			string utilsPath = assetPath.Substring(0, assetPath.Length-6) + "Utils.blend";

			Object mainUtilsAsset = AssetDatabase.LoadMainAssetAtPath(utilsPath);

			Transform[] utilLinks = findUtils(g.transform);

			//Debug.Log("---------found "+utilLinks.Length + " Links in "+assetPath);

			if(utilLinks.Length > 0 && !(mainUtilsAsset is GameObject) ){
				Debug.LogWarning("Found Utils reference in "+assetPath+" but no GameObject Asset at "+utilsPath);
			}
			else{

				foreach(Transform t in utilLinks){

					//Debug.Log("Found Utils reference in "+assetPath+" : "+t.name);

					string[] s = t.name.Trim().Split('_');

					int number=0;
					if(int.TryParse( s[s.Length-1],out number)){
						List<string> sList = new List<string>(s);
						sList.RemoveAt(s.Length-1);
						s = sList.ToArray();
					}


					string name = s[s.Length-1];
					string type = s[s.Length-2];

					if( name.Equals("L") || name.Equals("R") || name.Equals("l") || name.Equals("r")){
						name = s[s.Length-2];
						type = s[s.Length-3];
						//Debug.Log("lrprefix removed");
					}

					//Debug.Log(t.name +"  type: "+type);

					switch(type){
					case "mesh":
					case "Mesh":

						Transform trans = ((GameObject)mainUtilsAsset).transform;

						Transform linkTarget = findFirstNamed(trans, name); // just take the first match, ok?


						
						if(linkTarget == null){ //it might be a link to a blend with a single object
							if(trans.childCount == 0){
								MeshFilter mf = trans.GetComponent<MeshFilter>();
								if(mf!=null && mf.sharedMesh.name.Equals(name)){ //jap, that's it
									linkTarget = trans;
								}
							}
						}
						
						if(linkTarget != null){
							//Debug.Log("    Found Target for "+name+": "+linkTarget.name);
							
							Component[] comps = linkTarget.gameObject.GetComponents<Component>();
							foreach(Component comp in comps){
								if(! (comp is Transform) ){
									//Debug.Log("    it's got a "+comp.GetType() + " copying...");
									copyComponent(comp, t.gameObject);
								}
							}
						}
						else{
							//Debug.Log("couldn't find "+name+" in "+trans.name);
							Debug.LogWarning("couldn't find "+name+" in "+utilsPath);
						}							

						break;

					default:

						break;
					}
				}
			}
		}
	}

	private Component copyComponent(Component original, GameObject destination) {
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		if(copy!=null){
			EditorUtility.CopySerialized(original, copy);
		}
		return copy;
	}


	private Transform[] findUtils(Transform root){
		List<Transform> results = new List<Transform>();
		findUtilsRec(root, results);
		return results.ToArray();
	}

	private void findUtilsRec(Transform t, List<Transform> l){
		//Debug.Log("transform name: " + t.name);
		if(t.name.StartsWith("Utils_")){
			l.Add(t);
			//Debug.Log("added "+t.name);
		}
		else{
			foreach(Transform child	in t){
				findUtilsRec(child, l);
			}
		}
	}

	private Transform findFirstNamed(Transform t, string name){
		if(t.name.Equals(name)){
			return t;
		}
		foreach(Transform child in t.transform){
			Transform found = findFirstNamed(child, name);
			if(found!=null) return found;
		}
		return null;
	}

	private Transform findFirstRec(Transform t, string name){
		if(t.name.Equals(name) || (t.gameObject.GetComponent<MeshFilter>()!=null && t.gameObject.GetComponent<MeshFilter>().sharedMesh.name.Equals(name+ " Instance")) ){
			return t;
		}
		else{
			Debug.Log("not "+name+" = "+t.name);
			//Debug.Log(" and "+(t.gameObject.GetComponent<MeshFilter>()?t.gameObject.GetComponent<MeshFilter>().sharedMesh.name:" meshfilter is null") );
			foreach(Transform child in t){
				return findFirstRec(child, name);
			}
		}
		return null;
	}

	private void rotateMeshesRec(GameObject g){
		MeshFilter mf = g.GetComponent<MeshFilter>();
		if(mf!=null){
			//Debug.Log("rotating " + g.name);
			Quaternion rot = Quaternion.Euler(0f, 0f, 90f);
			Vector3[] verts = mf.sharedMesh.vertices;
			Vector3[] norms = mf.sharedMesh.normals;
			for(int i=0; i<verts.Length; i++){
				verts[i] = rot * verts[i];
				norms[i] = rot * norms[i];
			}
			mf.sharedMesh.vertices = verts;
			mf.sharedMesh.normals = norms;
		}
		
		foreach(Transform child in g.transform){
			rotateMeshesRec(child.gameObject);
		}
	}

}
