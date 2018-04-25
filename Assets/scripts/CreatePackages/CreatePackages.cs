#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using System.Globalization;
using System.Threading;

public class CreateAssetBundles : EditorWindow
{
    [MenuItem("Assets/Create package from selection")]
    static void CreatePackage()
    {
        EditorUtility.ClearProgressBar();
        string CultureName = Thread.CurrentThread.CurrentCulture.Name;
        CultureInfo culture = new CultureInfo(CultureName);
        culture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = culture;
        if (Application.isPlaying)
        {
            Debug.Log("Can't create packages if application is playing");
            return;
        }
        string path = "C:/Users/Romain/Documents/Unity/HypnoseARV3/Asset Packages";
        if (!Directory.Exists(path))
        {
            Debug.Log("path not found : " + path);
            return;
        }
        Object[] currentObjects = Selection.objects;

        // updating selfId
        int selfId = 1;
        if (File.Exists(path + "/" + "index.txt"))
        {
            string selfIdstring = File.ReadAllText(path + "/" + "index.txt");
            int.TryParse(selfIdstring, out selfId);
        }
        XDocument downloadListXml;
        try
        {
            downloadListXml = XDocument.Load(path + "/" + "downloadList.xml");

        }
        catch (FileNotFoundException e)
        {
            //file not found
            downloadListXml = new XDocument();
        }
        float progress;
        int counter = 0;
        foreach (Object curObj in currentObjects)
        {
            counter++;
            progress = (float)counter / currentObjects.Length;
            EditorUtility.DisplayProgressBar("Exporting objects to package... (" + Mathf.Round(progress * 100) + "%)", "Exporting object " + curObj.name, progress);
            if (curObj.GetType() == typeof(GameObject))
            {
                if (curObj == null)
                {
                    Debug.Log("No object selected");
                    return;
                }
                GameObject currentObject = (GameObject)curObj;

                // mesh
                MeshFilter mesh = currentObject.GetComponent<MeshFilter>();
                if (mesh == null)
                {
                    Debug.Log("Mesh not found");
                    return;
                }
                string meshPath = AssetDatabase.GetAssetPath(mesh.sharedMesh);
                string[] meshPathArray = meshPath.Split('/');
                string meshName = meshPathArray[meshPathArray.Length - 1];
                if (Path.GetExtension(meshPath).ToUpper() != ".OBJ") // different file format -> recreating .obj from mesh
                {
                    Debug.Log(meshPath);
                    meshName = Path.ChangeExtension(meshName, ".OBJ");
                    string exportPath = path + "/" + meshName;
                    MeshExport(mesh, exportPath);
                }
                else // no conversion needed
                {
                    File.Copy(meshPath, path + "/" + meshName, true); // overwrite file in destination folder
                }

                // texture
                Texture tex = currentObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
                if (tex == null)
                {
                    Debug.Log("Texture not found");
                    return;
                }
                string texPath = AssetDatabase.GetAssetPath(tex);
                string[] texPathArray = texPath.Split('/');
                string texName = texPathArray[texPathArray.Length - 1];
                File.Copy(texPath, path + "/" + texName, true); // overwrite file in destination folder

                // updating dowloadList
                XElement selfIdNode = new XElement("selfId", selfId);
                XElement textureNode = new XElement("texture", texName);
                XElement meshNode = new XElement("mesh", meshName);
                XElement node = new XElement("object", selfIdNode, textureNode, meshNode);
                XElement rootNode = downloadListXml.Element("downloadList");
                if (rootNode == null)
                {
                    rootNode = new XElement("downloadList", node);
                    downloadListXml.Add(rootNode);
                }
                else
                {
                    rootNode.Add(node);
                }

                Texture2D texture = RuntimePreviewGenerator.GenerateModelPreview(currentObject.transform, Quaternion.identity, 512, 512);
                Debug.Log(texture);
                RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(texture, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D preview = new Texture2D(texture.width, texture.height);
                preview.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                preview.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                if (preview == null)
                {
                    Debug.Log("can't generated thumbnail icon for object " + selfId);
                }
                else
                {
                    File.WriteAllBytes(path + "/" + selfId + "_thumbnail.png", preview.EncodeToPNG());
                }

            }
            selfId++;
        }

        downloadListXml.Save(path + "/" + "downloadList.xml");
        File.WriteAllText(path + "/" + "index.txt", selfId.ToString());
        Debug.Log("All " + currentObjects.Length + " object(s) have been prepared");
        EditorUtility.ClearProgressBar();
    }

    private PreviewRenderUtility mPrevRender;
    private Mesh mPreviewMesh;
    private Material mMat;
    //************************************************************************
    public void DrawRenderPreview(Rect r)
    {
            mPrevRender = new PreviewRenderUtility();
        mPrevRender.camera.transform.position = (Vector3)(-Vector3.forward * 8f);
        mPrevRender.camera.transform.rotation = Quaternion.identity;
        mPrevRender.camera.farClipPlane = 30;

        mPrevRender.lights[0].intensity = 0.5f;
        mPrevRender.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
        mPrevRender.lights[1].intensity = 0.5f;

        mPrevRender.BeginPreview(r, GUIStyle.none);
        mPrevRender.DrawMesh(mPreviewMesh, -Vector3.up * 0.5f, Quaternion.Euler(-30f, 0f, 0f) * Quaternion.Euler(0f, 60, 0f), mMat, 0);

        bool fog = RenderSettings.fog;
        Unsupported.SetRenderSettingsUseFogNoDirty(false);
        mPrevRender.camera.Render();
        Unsupported.SetRenderSettingsUseFogNoDirty(fog);
        Texture texture = mPrevRender.EndPreview();

        GUI.DrawTexture(r, texture);
    }
    //************************************************************************


    // Unity3D Scene OBJ Exporter // Author:  aaro4130 - modified by Romain Paris

    private static void MeshExport(MeshFilter mf, string exportPath)
    {
        bool applyScale = true;
        bool applyRotation = false;
        bool applyPosition = false;

        MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            if (mr.isPartOfStaticBatch)
            {
                return;
            }
        }

        //work on export
        StringBuilder sb = new StringBuilder();
        StringBuilder sbMaterials = new StringBuilder();
        sb.AppendLine("# Export of HypnoseAR project by David Libeau and Romain Paris");
        sb.AppendLine("# from Aaro4130 OBJ Exporter  - modified by Romain Paris");
        sb.AppendLine();
        string[] pth = exportPath.Split('/');
        sb.AppendLine("#");
        sb.AppendLine("# object " + pth[pth.Length-1]);
        sb.AppendLine("#");
        sb.AppendLine();

        //export the meshhh :3
        Mesh msh = mf.sharedMesh;
        int faceOrder = (int)Mathf.Clamp((mf.gameObject.transform.lossyScale.x * mf.gameObject.transform.lossyScale.z), -1, 1);

        //export vector data (FUN :D)!
        foreach (Vector3 vx in msh.vertices)
        {
            Vector3 v = vx;
            if (applyScale)
            {
                v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale);
            }

            if (applyRotation)
            {

                v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            }

            if (applyPosition)
            {
                v += mf.gameObject.transform.position;
            }
            v.x *= -1;
            sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
        }
        sb.AppendLine();
        foreach (Vector3 vx in msh.normals)
        {
            Vector3 v = vx;

            if (applyScale)
            {
                v = MultiplyVec3s(v, mf.gameObject.transform.lossyScale.normalized);
            }
            if (applyRotation)
            {
                v = RotateAroundPoint(v, Vector3.zero, mf.gameObject.transform.rotation);
            }
            v.x *= -1;
            sb.AppendLine("vn " + v.x + " " + v.y + " " + v.z);

        }
        sb.AppendLine();
        Debug.Log(msh.uv.Length);
        foreach (Vector2 v in msh.uv)
        {
            sb.AppendLine("vt " + v.x + " " + v.y);
        }
        sb.AppendLine();
        int triCount = 0;
        for (int j = 0; j < msh.subMeshCount; j++)
        {
            int[] tris = msh.GetTriangles(j);
            triCount += tris.Length;
            for (int t = 0; t < tris.Length; t += 3)
            {
                int idx2 = tris[t] + 1;
                int idx1 = tris[t + 1] + 1;
                int idx0 = tris[t + 2] + 1;
                if (faceOrder < 0)
                {
                    sb.AppendLine("f " + ConstructOBJString(idx2) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx0));
                }
                else
                {
                    sb.AppendLine("f " + ConstructOBJString(idx0) + " " + ConstructOBJString(idx1) + " " + ConstructOBJString(idx2));
                }

            }
        }
        sb.AppendLine("# 0 polygons - " + triCount + " triangles");

        //write to disk
        System.IO.File.WriteAllText(exportPath, sb.ToString());
    }

    private static string ConstructOBJString(int index)
    {
        string idxString = index.ToString();
        return idxString + "/" + idxString + "/" + idxString;
    }

    private static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    private static Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
}
#endif