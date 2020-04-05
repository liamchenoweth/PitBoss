import axios from 'axios';

function RequestConfig()
{
    return {
        
    }
}

export async function Get(url, params)
{
    var config = RequestConfig();
    config.method = "get";
    config.url = url;
    config.params = params
    return await axios(config);
}

export async function Post(url, body)
{
    var config = RequestConfig();
    config.method = "post";
    config.url = url;
    config.data = body;
    return await axios(config);
}

export async function Delete(url, body)
{
    var config = RequestConfig();
    config.method = "delete";
    config.url = url;
    config.data = body;
    return await axios(config);
}