using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cibbi.ToonyStandard.GitHubAPI
{

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
        public GitHubUserJSON author;
        public bool prerelease;
        public string created_at;
        public string published_at;
        public GitHubAssetJSON[] assets;
        public string tarball_url;
        public string zipball_url;
        public string body;
    }

    [Serializable]
    public class GitHubCommitJSON{
        public string sha;
        public string node_id;
        public GitHubCommitLiteJSON commit;
        public string url;
        public string html_url;
        public string comments_url;
        public GitHubUserJSON author;
        public GitHubUserJSON committer;
        public GitHubCommitParentJSON[] parents;
    }

    [Serializable]
    public class GitHubUserJSON{
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
        public GitHubUserJSON uploader;
        public string content_type;
        public string state;
        public int size;
        public int download_count;
        public string created_at;
        public string published_at;
        public string browser_download_url;
    }

    [Serializable]
    public class GitHubCommitLiteJSON{
        public GitHubCommitUserJSON author;
        public GitHubCommitUserJSON committer;
        public GitHubCommitTreeJSON tree;
        public string message;
        public string url;
        public int comment_count;
        public GitHubVerificationJSON verification;
    }

    [Serializable]
    public class GitHubCommitUserJSON{
        public string name;
        public string email;
        public string date;
    }

    [Serializable]
    public class GitHubCommitTreeJSON{
        public string sha;
        public string url;
    }

    [Serializable]
    public class GitHubCommitParentJSON{
        public string sha;
        public string url;
        public string html_url;
    }
    [Serializable]
    public class GitHubVerificationJSON{
        public bool verified;
        public string reason;
        public string signature;
        public string payload;
    }
}