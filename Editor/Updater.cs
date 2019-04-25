#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using System;

public class ToonyStandardUpdater : EditorWindow {

    private static int screenWidth=250;
    private static int screenHeight=100;

    bool versionCheck=false;
    bool requested=false;
    bool requestDone=false;
    bool downloading=false;
    bool error=false;
    
    bool UpdateButtonChecked=false;
    
    
    string errorMessage="";


    UnityWebRequest request;
    DownloadHandlerBuffer response;
    DownloadHandlerFile file;

    GitHubReleaseJSON githubJSON;

    Texture2D icon;
    [MenuItem("Window/Toony Standard/Updater")]
    static void Init()
    {
        UnityEditor.EditorWindow window = GetWindow(typeof(ToonyStandardUpdater));
        window.position = new Rect(Screen.currentResolution.width/2-screenWidth/2, Screen.currentResolution.height/4-screenHeight/2, screenWidth, screenHeight);
        window.Show();
    }

    void Update()
    {
        if(!versionCheck)
        {
            if(!requested)
            {
                request= new UnityWebRequest("https://api.github.com/repos/Cibbi/Toony-standard/releases/latest");
                request.method=UnityWebRequest.kHttpVerbGET;
                response = new DownloadHandlerBuffer();
                request.downloadHandler = response;
                request.SendWebRequest();
                requested=true;
                requestDone=false;
            }
            if (!requestDone)
            { 
                if(request.isHttpError||request.isNetworkError)
                {
                    errorMessage="Couldn't retrieve update information, please retry later";
                    
                    error=true;
                    Debug.Log(request.error);
                    requestDone=true;
                    versionCheck=true;
                }
                if(request.isDone)
                {
                    requestDone=true;
                }
            }
            if(requestDone&&!error)
            {
                string data=response.text;

                githubJSON=JsonUtility.FromJson<GitHubReleaseJSON>(data);
                versionCheck=true;

            }
            
        }
        else
        {
            if(!error)
            {
                if(!downloading&&UpdateButtonChecked)
                {

                    request = new UnityWebRequest(githubJSON.assets[0].browser_download_url);
                    request.method=UnityWebRequest.kHttpVerbGET;
                    file= new DownloadHandlerFile(Application.dataPath+"/toonyStandard.unitypackage");
                    file.removeFileOnAbort=true;
                    request.downloadHandler=file;
                    request.SendWebRequest();
                    downloading=true; 
                    requestDone=false; 
                    UpdateButtonChecked=false;

                }
                else
                {
                    if(request.isDone && !requestDone)
                    {
                        Debug.Log("Done!");
                        AssetDatabase.ImportPackage(Application.dataPath+"/toonyStandard.unitypackage",true);
                        AssetDatabase.Refresh();
                        AssetDatabase.DeleteAsset("Assets/toonyStandard.unitypackage");
                        requestDone=true;
                        this.Close();
                    }
                    if(request.isNetworkError)
                    {
                        Debug.Log("network error");
                    }
                    if(request.isHttpError)
                    {
                        Debug.Log("http error");
                    }
                }
            }
        }
    }
    

    void OnGUI()
    {
        if(!versionCheck)
        {
            EditorGUILayout.LabelField("Checking for update");
        }
        else
        {
            if(error)
            {
                EditorGUILayout.LabelField(errorMessage);
            }
            else
            {
                EditorGUILayout.LabelField("New version found: "+githubJSON.tag_name);
                if(!downloading)
                {
                    if(GUILayout.Button("Update"))
                    {     
                        UpdateButtonChecked=true;
                    }
                }
                else
                {
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false),request.downloadProgress,(request.downloadProgress*100)+"%");
                }
            }
        }
		
    }
    
    void OnInspectorUpdate()
    {
        if(!versionCheck||(versionCheck&&(downloading||requestDone)))
            Repaint();
    }

    /* public void Start()
    {
        

        

        firstCycle=false;
    }*/

}

[Serializable]
public class GitHubReleaseJSON{
    public string url;
    public string assets_url;
    public string upload_url;
    public string html_url;
    public int id;
    public string node_id;
    public string tag_name;
    public string target_commitish;
    public string name;
    public bool draft;
    public GitHubAuthorJSON author;
    public bool prerelease;
    public string created_at;
    public string published_at;
    public GitHubAssetJSON[] assets;
    public string tarball_url;
    public string zipball_url;
    public string body;
}

[Serializable]
public class GitHubAuthorJSON{
    public string login;
    public int id;
    public string node_id;
    public string avatar_url;
    public string gravatar_id;
    public string url;
    public string html_url;
    public string followers_url;
    public string following_url;
    public string gists_url;
    public string starred_url;
    public string subscriptions_url;
    public string organizations_url;
    public string repos_url;
    public string events_url;
    public string received_events_url;
    public string type;
    public string site_admin;
}

[Serializable]
public class GitHubAssetJSON{
    public string url;
    public int id;
    public string node_id;
    public string name;
    public string label;
    public GitHubAuthorJSON uploader;
    public string content_type;
    public string state;
    public int size;
    public int download_count;
    public string created_at;
    public string published_at;
    public string browser_download_url;
}


#endif