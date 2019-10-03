﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

public class AssetsExporter : MonoBehaviour
{

    public static string modelsStorageURL = "assetMedia/models/";
    public static string propsStorageURL = "assetMedia/props/";
    public static string imagesStorageURL = "assetMedia/images/";
    public static string modelStorageName = "original";


    Dictionary<string, string> thumbnails = new Dictionary<string, string>();
    public GameObject modelsParent;
    bool headShotSpaceAvailable;




    protected void UploadFileTo(string fileName, string filePath, string uploadStoragePath, string fileTag, bool isBundle)
	{
        //Get screenshot and upload image
        //StartCoroutine(LoadModelForHeadshot(fileName));

        //Upload Original FBX to DB
        UploadOriginalFileToStorage(fileName, filePath, uploadStoragePath, fileTag);

        //Upload Asset Bundle to DB

	}

    IEnumerator LoadModelForHeadshot(string modelName)
	{
        //Instantiate model from resources
        //GameObject model = (GameObject)Intansciate;
        Debug.Log(modelName);
        yield return new WaitUntil(() => headShotSpaceAvailable == true);
        headShotSpaceAvailable = false;
        GameObject model = (GameObject)Instantiate(Resources.Load(modelName));
        model.transform.SetParent(modelsParent.transform);
        Vector3 modelRotation = Vector3.zero;
        modelRotation.x = model.transform.localRotation.x;
        modelRotation.y = model.transform.localRotation.y + 180f;
        modelRotation.z = model.transform.localRotation.z;
        model.transform.localRotation = Quaternion.Euler(modelRotation);
        yield return new WaitForEndOfFrame();
        ScreenCapture.GrabPixelsOnPostRender(modelName+ Time.time.ToString());
        yield return new WaitForEndOfFrame();
        Destroy(model);

    }


    protected void UploadOriginalFileToStorage(string fileName, string filePath, string uploadStoragePath, string fileTag)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        string path = uploadStoragePath;
        // Get a reference to the storage service, using the default Firebase App
        Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        // Create a reference with an initial file path and name
        Firebase.Storage.StorageReference path_reference =
          storage.GetReference(path);

        // Upload the file to the path "images/rivers.jpg"
        path_reference.PutBytesAsync(bytes)
          .ContinueWith((Task<Firebase.Storage.StorageMetadata> task) => {
              if (task.IsFaulted || task.IsCanceled)
              {
                  Debug.Log(task.Exception.ToString());
                  // Uh-oh, an error occurred!
              }
              else
              {
                  // Metadata contains file metadata such as size, content-type, and download URL.
                  Firebase.Storage.StorageMetadata metadata = task.Result;
                  //string download_url = metadata.DownloadUrl.ToString();
                  Debug.Log("Finished uploading...");
                  //Debug.Log("download url = " + download_url);
              }
          });


    }

    protected void ExportFiles()
    {
        var info = new DirectoryInfo("Assets/Resources");
        var fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            
            //end of directory path, start of file name
            int fileNamePos = file.ToString().LastIndexOf("/", System.StringComparison.CurrentCulture);
            //end of file name, start of file extension
            int fileExtPos = file.ToString().LastIndexOf(".", System.StringComparison.CurrentCulture);

            //parent directory with trailing slash
            string filePath = file.ToString().Substring(0, fileNamePos + 1);
            //isolated file name
            string fileName = file.ToString().Substring(fileNamePos + 1, fileExtPos - filePath.Length);
            //extension with "."
            string fileExt = file.ToString().Substring(fileExtPos);

            if (fileExt == ".fbx" || fileExt == ".FBX" || fileExt == ".OBJ" || fileExt == ".obj") 
             
            {
                Debug.Log("Found 3D Model File: " + fileName);
                string uploadStoragePath = AssetsExporter.modelsStorageURL + fileName + "/" + AssetsExporter.modelStorageName + fileExt.ToLower();
                UploadFileTo(fileName, file.ToString(), uploadStoragePath, "", false);
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //UploadFileTo("X Bot", "dummy", "", false);
        // Set this before calling into the realtime database.
        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://YOUR-FIREBASE-APP.firebaseio.com/");

        // Get the root reference location of the database.
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.Auth.Credential credential =
    Firebase.Auth.EmailAuthProvider.GetCredential("developer@amirbaradaran.com", "abcd1234");
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });

        ExportFiles();
    }

    // Update is called once per frame
    void Update()
    {
        if (modelsParent.transform.childCount == 0)
        {
            headShotSpaceAvailable = true;
        }
       
    }
}
