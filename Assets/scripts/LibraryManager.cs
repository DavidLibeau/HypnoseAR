using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

#if UNITY_EDITOR
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
#endif

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
#endif

public class LibraryManager : MonoBehaviour
{

    public static LibraryManager Instance;
    public Text updateButtonText;
    public Mutex readingInFiles = new Mutex();

#if WINDOWS_UWP
    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
#else
    string localFolder = "C:/Users/Romain/Documents/Unity/HypnoseARV3/Dump";
#endif

    // Use this for initialization
    void Start()
    {
        Instance = this;

        // to be removed
    }


    public IEnumerator DownloadLibFile(string parameters, string fileName)
    {
        string completeUrl = MainPanel.Instance.Url + parameters;
        Debug.Log(completeUrl);
        UnityWebRequest uwr = UnityWebRequest.Get(completeUrl);
        AsyncOperation request = uwr.SendWebRequest();
        while (!request.isDone)
        {
            float progress = uwr.downloadProgress * 100;
            updateButtonText.text = "Downloading... " + progress.ToString("F0") + "%";
            yield return null;
        }
        Debug.Log("request response code : " + uwr.responseCode);
        updateButtonText.text = "Update Library";
        if (uwr.responseCode != 200)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("Download progress :" + uwr.downloadProgress);
            byte[] data = uwr.downloadHandler.data;
            WriteData(data, fileName);
        }
    }


    public async void WriteData(byte[] data, string fileName)
    {
        Debug.Log("Writing data to " + localFolder);
#if WINDOWS_UWP
        try // in case several holograms try to write at the same time a same file (eg. texture)
        {
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, data);
        }
        catch (Exception e)
        {
            Debug.Log("unauthorized access to file or file not found : " + fileName);
        }
        
        //CreateObjectAsync("1.xml");
#else
        File.WriteAllBytes(localFolder + "/"+ fileName, data);
        //CreateObjectAsync("");
#endif
    }


    public async Task<GameObject> CreateObjectAsync(string name)
    {
#if WINDOWS_UWP
        Debug.Log("LocalFolder path : " + localFolder.Path);
#else
        Debug.Log("LoadObjectAsync LocalFolder path : " +  localFolder + "/" + name);
#endif
        // load XML info file
#if WINDOWS_UWP
        XDocument infoXml = XDocument.Load(localFolder.Path + "/" + name);
#else
        string info = File.ReadAllText(localFolder + "/" + name);
        XDocument infoXml = XDocument.Parse(info);
#endif
        Debug.Log(infoXml);
        XElement rootNode = infoXml.Descendants("root").First<XElement>();  
        string objName = rootNode.Element("mesh").Value;
        string texName = rootNode.Element("texture").Value;

        //execution concurrente
        readingInFiles.WaitOne();
        // load obj
        objName = objName.Split('/').Last();
        texName = texName.Split('/').Last();
        Task<List<GameObject>> objTask = LoadOBJ(objName);
        Debug.Log("objTask Launched");
        // load texture(s)
        Task<Texture2D> texTask = LoadTexture(texName);
        Debug.Log("texTask Launched");
        //while (objTask.Status != TaskStatus.RanToCompletion && texTask.Status != TaskStatus.RanToCompletion)
        //{
        //    Debug.Log("objTask status : " + objTask.Status);
        //    Debug.Log("texTask status : " + texTask.Status);
        //    await Task.Delay(500);
        //}
        objTask.Wait();
        texTask.Wait();
        Debug.Log("objTask status : " + objTask.Status);
        Debug.Log("texTask status : " + texTask.Status);
        readingInFiles.ReleaseMutex();

        Texture2D tex = texTask.Result;
        List<GameObject> gOList = objTask.Result;
        GameObject newObj;
        if (gOList[0] != null)
        {
            newObj = gOList[0];
        }
        else
        {
            Debug.Log("object is null");
            newObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        // load shader/material
        // using standard shader until I find another solution
        newObj.name = rootNode.Element("id").Value;
        //GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newObj.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        return newObj;
    }

    public async void PutUpdateLibInfo()
    {
        DateTime date = DateTime.Now;
#if WINDOWS_UWP
        StorageFile file = await localFolder.CreateFileAsync("log.xml", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteBytesAsync(file, Encoding.ASCII.GetBytes(date.ToString()));
#else
        File.WriteAllBytes(localFolder + "/" + "log.xml", Encoding.ASCII.GetBytes(date.ToString()));
#endif
    }

    public DateTime GetUpdateLibInfo()
    {
        string dateString;
#if WINDOWS_UWP
        try
        {
            dateString = File.ReadAllText(localFolder.Path + "/" + "log.xml");
        }
        catch
        {
            dateString = DateTime.MinValue.ToString();
        }
#else
        dateString = File.ReadAllText(localFolder + "/" + "log.xml");
#endif
        DateTime date;
        DateTime.TryParse(dateString, out date);
        return date;
    }

    /*
     * From here to the bottom the source code comes from Runtime OBJ importer 
     * (C) 2015 AARO4130 PARTS OF TGA LOADING CODE (C) 2013 mikezila
     * Some parts have been modified -> async
    */



    // load obj file
    struct OBJFace
    {
        public string materialName;
        public string meshName;
        public int[] indexes;
    }

    public async Task<List<GameObject>> LoadOBJ(string fileName)
    {
        if (fileName.Equals(""))
        {
            //no name
            return null;
        }
        string meshName = fileName.Split('.').First();

        bool hasNormals = false;
        //OBJ LISTS
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //UMESH LISTS
        List<Vector3> uvertices = new List<Vector3>();
        List<Vector3> unormals = new List<Vector3>();
        List<Vector2> uuvs = new List<Vector2>();
        //MESH CONSTRUCTION
        List<string> materialNames = new List<string>();
        List<string> objectNames = new List<string>();
        Dictionary<string, int> hashtable = new Dictionary<string, int>();
        List<OBJFace> faceList = new List<OBJFace>();
        string cmaterial = "default";
        string cmesh = "default";
        //CACHE
        Material[] materialCache = null;
        //save this info for later
        FileInfo OBJFileInfo = new FileInfo(fileName);
#if WINDOWS_UWP
        var item = localFolder.TryGetItemAsync(fileName);
        if (item == null)
        {
            // file not found
            return null;
        }
        //StorageFile file = await localFolder.GetFileAsync(fileName); // NE MARCHE PAS
        //IList<string> lines = File.ReadAllLines(file.Path);

        IList<string> lines = File.ReadAllLines(localFolder.Path + "/" + fileName);
        string[] allLines = lines.ToArray();
#else
        string[] allLines = File.ReadAllLines(localFolder + "/" + fileName);
#endif
        foreach (string ln in allLines)
        {
            if (ln.Length > 0 && ln[0] != '#')
            {
#if WINDOWS_UWP
                string l = ln.Trim().Replace("  ", " ");
#else
                string l = ln.Trim().Replace("  ", " ").Replace(".",",");
#endif
                string[] cmps = l.Split(' ');
                string data = l.Remove(0, l.IndexOf(' ') + 1);

                //if (cmps[0] == "mtllib")
                //{
                //    //load cache
                //    string pth = OBJGetFilePath(data, OBJFileInfo.Directory.FullName + Path.DirectorySeparatorChar, meshName);
                //    if (pth != null)
                //        materialCache = LoadMTLFile(pth);

                //}
                if ((cmps[0] == "g" || cmps[0] == "o"))
                {
                    cmesh = data;
                    if (!objectNames.Contains(cmesh))
                    {
                        objectNames.Add(cmesh);
                    }
                }
                else if (cmps[0] == "usemtl")
                {
                    cmaterial = data;
                    if (!materialNames.Contains(cmaterial))
                    {
                        materialNames.Add(cmaterial);
                    }
                }
                else if (cmps[0] == "v")
                {
                    //VERTEX
                    vertices.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vn")
                {
                    //VERTEX NORMAL
                    normals.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vt")
                {
                    //VERTEX UV
                    uvs.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "f")
                {
                    int[] indexes = new int[cmps.Length - 1];
                    for (int i = 1; i < cmps.Length; i++)
                    {
                        string felement = cmps[i];
                        int vertexIndex = -1;
                        int normalIndex = -1;
                        int uvIndex = -1;
                        if (felement.Contains("//"))
                        {
                            //doubleslash, no UVS.
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (felement.Count(x => x == '/') == 2)
                        {
                            //contains everything
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (!felement.Contains("/"))
                        {
                            //just vertex inedx
                            vertexIndex = int.Parse(felement) - 1;
                        }
                        else
                        {
                            //vertex and uv
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                        }
                        string hashEntry = vertexIndex + "|" + normalIndex + "|" + uvIndex;
                        if (hashtable.ContainsKey(hashEntry))
                        {
                            indexes[i - 1] = hashtable[hashEntry];
                        }
                        else
                        {
                            //create a new hash entry
                            indexes[i - 1] = hashtable.Count;
                            hashtable[hashEntry] = hashtable.Count;
                            uvertices.Add(vertices[vertexIndex]);
                            if (normalIndex < 0 || (normalIndex > (normals.Count - 1)))
                            {
                                unormals.Add(Vector3.zero);
                            }
                            else
                            {
                                hasNormals = true;
                                unormals.Add(normals[normalIndex]);
                            }
                            if (uvIndex < 0 || (uvIndex > (uvs.Count - 1)))
                            {
                                uuvs.Add(Vector2.zero);
                            }
                            else
                            {
                                uuvs.Add(uvs[uvIndex]);
                            }

                        }
                    }
                    if (indexes.Length < 5 && indexes.Length >= 3)
                    {
                        OBJFace f1 = new OBJFace();
                        f1.materialName = cmaterial;
                        f1.indexes = new int[] { indexes[0], indexes[1], indexes[2] };
                        f1.meshName = cmesh;
                        faceList.Add(f1);
                        if (indexes.Length > 3)
                        {

                            OBJFace f2 = new OBJFace();
                            f2.materialName = cmaterial;
                            f2.meshName = cmesh;
                            f2.indexes = new int[] { indexes[2], indexes[3], indexes[0] };
                            faceList.Add(f2);
                        }
                    }
                }
            }
        }
        if (objectNames.Count == 0)
        {
            objectNames.Add("default");
        }
        if(materialNames.Count == 0)
        {
            materialNames.Add("default");
        }
        //build objects
        List<GameObject> objectList = new List<GameObject>(objectNames.Count);

        foreach (string obj in objectNames)
        {
            GameObject subObject = new GameObject(obj);
            subObject.transform.localScale = new Vector3(-1, 1, 1);
            //Create mesh
            Mesh m = new Mesh();
            m.name = obj;
            //LISTS FOR REORDERING
            List<Vector3> processedVertices = new List<Vector3>();
            List<Vector3> processedNormals = new List<Vector3>();
            List<Vector2> processedUVs = new List<Vector2>();
            List<int[]> processedIndexes = new List<int[]>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            //POPULATE MESH
            List<string> meshMaterialNames = new List<string>();

            OBJFace[] ofaces = faceList.Where(x => x.meshName == obj).ToArray();
            foreach (string mn in materialNames)
            {
                OBJFace[] faces = ofaces.Where(x => x.materialName == mn).ToArray();
                if (faces.Length > 0)
                {
                    int[] indexes = new int[0];
                    foreach (OBJFace f in faces)
                    {
                        int l = indexes.Length;
                        System.Array.Resize(ref indexes, l + f.indexes.Length);
                        System.Array.Copy(f.indexes, 0, indexes, l, f.indexes.Length);
                    }
                    meshMaterialNames.Add(mn);
                    if (m.subMeshCount != meshMaterialNames.Count)
                        m.subMeshCount = meshMaterialNames.Count;

                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int idx = indexes[i];
                        //build remap table
                        if (remapTable.ContainsKey(idx))
                        {
                            //ezpz
                            indexes[i] = remapTable[idx];
                        }
                        else
                        {
                            processedVertices.Add(uvertices[idx]);
                            processedNormals.Add(unormals[idx]);
                            processedUVs.Add(uuvs[idx]);
                            remapTable[idx] = processedVertices.Count - 1;
                            indexes[i] = remapTable[idx];
                        }
                    }

                    processedIndexes.Add(indexes);
                }
                else
                {

                }
            }

            //apply stuff
            m.vertices = processedVertices.ToArray();
            m.normals = processedNormals.ToArray();
            m.uv = processedUVs.ToArray();

            for (int i = 0; i < processedIndexes.Count; i++)
            {
                m.SetTriangles(processedIndexes[i], i);
            }

            if (!hasNormals)
            {
                m.RecalculateNormals();
            }
            m.RecalculateBounds();
            ;

            MeshFilter mf = subObject.AddComponent<MeshFilter>();
            MeshRenderer mr = subObject.AddComponent<MeshRenderer>();

            Material[] processedMaterials = new Material[meshMaterialNames.Count];
            for (int i = 0; i < meshMaterialNames.Count; i++)
            {

                if (materialCache == null)
                {
                    processedMaterials[i] = new Material(Shader.Find("Standard"));
                }
                else
                {
                    Material mfn = Array.Find(materialCache, x => x.name == meshMaterialNames[i]); ;
                    if (mfn == null)
                    {
                        processedMaterials[i] = new Material(Shader.Find("Standard"));
                    }
                    else
                    {
                        processedMaterials[i] = mfn;
                    }

                }
                processedMaterials[i].name = meshMaterialNames[i];
            }

            mr.materials = processedMaterials;
            mf.mesh = m;
            objectList.Add(subObject);
        }
        return objectList;
    }

    public static Vector3 ParseVectorFromCMPS(string[] cmps)
    {
        float x = float.Parse(cmps[1]);
        float y = float.Parse(cmps[2]);
        if (cmps.Length == 4)
        {
            float z = float.Parse(cmps[3]);
            return new Vector3(x, y, z);
        }
        return new Vector2(x, y);
    }


    //load texture file (tga or jpg, png)
    public async Task<Texture2D> LoadTexture(string fileName, bool normalMap = false)
    {

#if WINDOWS_UWP
        var item = await localFolder.TryGetItemAsync(fileName);
        if (item == null)
        {
            return null;
        }
        //StorageFile file = await localFolder.GetFileAsync(fileName);
        //IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);

#endif
        string ext = fileName.Split('.').Last().ToLower();
        if (ext == "png" || ext == "jpg")
        {
            Texture2D t2d = new Texture2D(1, 1);
#if WINDOWS_UWP
            byte[] data = File.ReadAllBytes(localFolder.Path + "/" + fileName);
            //byte[] data = new byte[fileStream.Size];
            //await fileStream.ReadAsync(data.AsBuffer(), (uint)fileStream.Size, InputStreamOptions.None);
            //fileStream.Dispose();
#else
            byte[] data = File.ReadAllBytes(localFolder + "/" + fileName);
#endif
            t2d.LoadImage(data);
            //if (normalMap)
            //    SetNormalMap(ref t2d);
            return t2d;
        }
        //else if (ext == ".dds")
        //{
        //    Texture2D returnTex = LoadDDSManual(fn);
        //    if (normalMap)
        //        SetNormalMap(ref returnTex);
        //    return returnTex;
        //}
        else if (ext == "tga")
        {
#if WINDOWS_UWP
            FileStream fileStream = File.OpenRead(localFolder.Path + "/" + fileName);
            Texture2D returnTex = LoadTGA(fileStream);
#else
            Stream stream = File.OpenRead(localFolder + "/" + fileName) as Stream;
            Texture2D returnTex = LoadTGA(stream);
#endif
            //if (normalMap)
            //    SetNormalMap(ref returnTex);
            return returnTex;
        }
        else
        {
            Debug.Log("texture not supported : " + fileName);
        }
        return null;
    }

    public Texture2D LoadTGA(Stream TGAStream)
    {
        using (BinaryReader r = new BinaryReader(TGAStream))
        {
            // Skip some header info we don't care about.
            // Even if we did care, we have to move the stream seek point to the beginning,
            // as the previous method in the workflow left it at the end.
            r.BaseStream.Seek(12, SeekOrigin.Begin);

            short width = r.ReadInt16();
            short height = r.ReadInt16();
            int bitDepth = r.ReadByte();
            // Skip a byte of header information we don't care about.
            r.BaseStream.Seek(1, SeekOrigin.Current);
            Debug.Log("texture size : " + width + "/" + height);
            Texture2D tex = new Texture2D(width, height);
            Color32[] pulledColors = new Color32[width * height];

            if (bitDepth == 32)
            {
                for (int i = 0; i < width * height; i++)
                {
                    byte red = r.ReadByte();
                    byte green = r.ReadByte();
                    byte blue = r.ReadByte();
                    byte alpha = r.ReadByte();

                    pulledColors[i] = new Color32(blue, green, red, alpha);
                }
            }
            else if (bitDepth == 24)
            {
                for (int i = 0; i < width * height; i++)
                {
                    byte red = r.ReadByte();
                    byte green = r.ReadByte();
                    byte blue = r.ReadByte();

                    pulledColors[i] = new Color32(blue, green, red, 1);
                }
            }
            else
            {
                throw new Exception("TGA texture had non 32/24 bit depth.");
            }

            tex.SetPixels32(pulledColors);
            tex.Apply();
            return tex;

        }
    }
}
