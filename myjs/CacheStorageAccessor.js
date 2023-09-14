async function openCacheStorage() {
    return await window.caches.open("SqliteWasmHelper");
}

function createRequest(url, method, body = "") {
    let requestInit =
    {
        method: method
    };

    if (body != "") {
        requestInit.body = body;
    }

    let request = new Request(url, requestInit);

    return request;
}

export async function store(url, method, body = "", responseString) {
    let blazorSchoolCache = await openCacheStorage();
    let request = createRequest(url, method, body);
    let response = new Response(responseString);
    await blazorSchoolCache.put(request, response);
}

export async function storesqlite(url, method, body = "", sqliteBytes) {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open('SqliteWasmHelper')
    };

    const db = window.sqlitedb;

    if (!db.init) {

        db.init = true;

       

    }


    const data = sqliteBytes;

    const blob = new Blob([data], {
        type: 'application/octet-stream',
        ok: true,
        status: 200
    });

    const headers = new Headers({
        'content-length': blob.size
    });

    const response = new Response(blob, {
        headers
    });

    await db.cache.put(url, response);
}


export async function get(url, method, body = "") {
    let blazorSchoolCache = await openCacheStorage();
    let request = createRequest(url, method, body);
    let response = await blazorSchoolCache.match(request);

    if (response == undefined) {
        return "";
    }

    let result = await response.text();

    return result;
}

export async function remove(url, method, body = "") {
    let blazorSchoolCache = await openCacheStorage();
    let request = createRequest(url, method, body);
    await blazorSchoolCache.delete(request);
}

export async function removeAll() {
    let blazorSchoolCache = await openCacheStorage();
    let requests = await blazorSchoolCache.keys();

    for (let i = 0; i < requests.length; i++) {
        await blazorSchoolCache.delete(requests[i]);
    }
}

export async function retrieveFileFromCache(url, method, body = "") {
    try {
        let blazorSchoolCache = await openCacheStorage();
        let request = createRequest(url, method, body);
        let response = await blazorSchoolCache.match(request);

        if (!response) {
            throw new Error("File not found in cache");
        }

        const blob = await response.blob();
        return await blobToUint8Array(blob);
    } catch (error) {
        console.error("Error retrieving file from cache:", error);
        throw error; // Rethrow the error for Blazor to handle
    }
}


function blobToUint8Array(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
            const uint8Array = new Uint8Array(reader.result);
            resolve(uint8Array);
        };
        reader.onerror = reject;
        reader.readAsArrayBuffer(blob);
    });
}


export async function synchronizeDbWithCache(file) {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open('SqliteWasmHelper')
    };

    const db = window.sqlitedb;

    const backupPath = `/${file}`;
    const cachePath = `/data/cache/${file.substring(0, file.indexOf('_bak'))}`;


    if (window.Module.FS.analyzePath(backupPath).exists) {

        const waitFlush = new Promise((done, _) => {
            setTimeout(done, 10);
        });

        await waitFlush;

        const data = window.Module.FS.readFile(backupPath);

        const blob = new Blob([data], {
            type: 'application/octet-stream',
            ok: true,
            status: 200
        });

        const headers = new Headers({
            'content-length': blob.size
        });

        const response = new Response(blob, {
            headers
        });

        await db.cache.put(cachePath, response);

        window.Module.FS.unlink(backupPath);

        return 1;
    }
    return -1;
}