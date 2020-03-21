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