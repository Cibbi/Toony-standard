// This entire file is just a conglomerate of classes used in conjunction with JsonUtility to fetch the response info from the Github api

using System;

namespace Cibbi.ToonyStandard.GithubAPI
{

    [Serializable]
    public class GithubReleaseJSON{
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
        public GithubUserJSON author;
        public bool prerelease;
        public string created_at;
        public string published_at;
        public GithubAssetJSON[] assets;
        public string tarball_url;
        public string zipball_url;
        public string body;
    }

    [Serializable]
    public class GithubCommitJSON{
        public string sha;
        public string node_id;
        public GithubCommitLiteJSON commit;
        public string url;
        public string html_url;
        public string comments_url;
        public GithubUserJSON author;
        public GithubUserJSON committer;
        public GithubCommitParentJSON[] parents;
    }

    [Serializable]
    public class GithubUserJSON{
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
    public class GithubAssetJSON{
        public string url;
        public int id;
        public string node_id;
        public string name;
        public string label;
        public GithubUserJSON uploader;
        public string content_type;
        public string state;
        public int size;
        public int download_count;
        public string created_at;
        public string published_at;
        public string browser_download_url;
    }

    [Serializable]
    public class GithubCommitLiteJSON{
        public GithubCommitUserJSON author;
        public GithubCommitUserJSON committer;
        public GithubCommitTreeJSON tree;
        public string message;
        public string url;
        public int comment_count;
        public GithubVerificationJSON verification;
    }

    [Serializable]
    public class GithubCommitUserJSON{
        public string name;
        public string email;
        public string date;
    }

    [Serializable]
    public class GithubCommitTreeJSON{
        public string sha;
        public string url;
    }

    [Serializable]
    public class GithubCommitParentJSON{
        public string sha;
        public string url;
        public string html_url;
    }
    [Serializable]
    public class GithubVerificationJSON{
        public bool verified;
        public string reason;
        public string signature;
        public string payload;
    }
}