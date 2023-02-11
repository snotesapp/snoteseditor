
export function GetNCTitle(element) {
    return element.innerHTML;

}


export function CloseModal(modalId) {
    $(modalId).modal('hide');

    
}


export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export function copyTextToClipboard(text) {
    navigator.clipboard.writeText(text)
        .catch(function (error) {
            alert(error);
        });
}

export function focusOnSummaryPannel() {
    document.getElementById('summarypannel').scrollIntoView({ behavior: 'smooth' });
   
}

//destinationImage.addEventListener('click', pasteImage);

function blobToBase64(blob) {
    return new Promise((resolve, _) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.readAsDataURL(blob);
    });
}




export async function pasteImage() {
    try {
        
        const permission = await navigator.permissions.query({ name: 'clipboard-read' });
        if (permission.state === 'denied') {
            throw new Error('Not allowed to read clipboard.');
        }
       
        const clipboardContents = await navigator.clipboard.read();
        for (const item of clipboardContents) {
            if (!item.types.includes('image/png')) {
                throw new Error('Clipboard contains non-image data.');
            }
            const blob = await item.getType('image/png');

            return blobToBase64(blob);
        }
    }
    catch (error) {
        console.error(error.message);
    }
}


export function enablepanzoompg() {

    const matches = document.querySelectorAll("div.position-relative, img");
    matches.forEach((elem) => {
        const panzoom = Panzoom(elem, {
            maxScale: 5,
            canvas: true,
          
            contain: 'inside',
            disableZoom: false
        });

       

    });


}

export function enablepanzoom2(imgelementid, xValue, yValue) {

    const imgelement = document.getElementById(imgelementid)

    const panzoom = Panzoom(imgelement, {
        maxScale: 5,
        contain: 'inside',
        canvas: true
    })

    
    imgelement.parentElement.addEventListener('wheel', panzoom.zoomWithWheel)

    return panzoom;

}

export function enablepanzoom(imgelementid, zoominbtn, zoomoutbtn) {

    const imgelement = document.getElementById(imgelementid)

    const panzoom = Panzoom(imgelement, {
        maxScale: 5,
        contain: 'inside'
    })
  
    /*
    panzoom.pan(1, 1)
    panzoom.zoom(2, { animate: true })
    */

 
    zoominbtn.addEventListener('click', panzoom.zoomIn)
    zoomoutbtn.addEventListener('click', panzoom.zoomOut)
    wheelzoom = imgelement.parentElement.addEventListener('wheel', panzoom.zoomWithWheel)


    return panzoom;
  
}

export function disablepanzoom2(panzoomobject, imgelementid) {

    const imgelement = document.getElementById(imgelementid)

    panzoomobject.destroy()
   
    imgelement.parentElement.removeEventListener('wheel', panzoomobject.zoomWithWheel)

}

export function disablepanzoom(panzoomobject, imgelementid, zoominbtn, zoomoutbtn) {
    const imgelement = document.getElementById(imgelementid)

    panzoomobject.destroy()
    zoominbtn.removeEventListener('click', panzoomobject.zoomIn)
    zoomoutbtn.removeEventListener('click', panzoomobject.zoomOut)
    imgelement.parentElement.removeEventListener('wheel', panzoomobject.zoomWithWheel)

}

export function getpanzoomdata(panzoomobject, imgelementid) {

    const imgelement = document.getElementById(imgelementid)
    var pandata = panzoomobject.getPan()
    var scaledata = panzoomobject.getScale()
    var jsondata = { "x": parseInt( pandata.x), "y": parseInt( pandata.y), "scale": scaledata }
    imgelement.parentElement.removeEventListener('wheel', panzoomobject.zoomWithWheel)

    return jsondata
  
}

export function zoomInelement(elementid) {
    /*
    const elem = document.getElementById(elementid)
    const panzoom = Panzoom(elem, {
        maxScale: 5
    })
    panzoom.pan(10, 10)
    panzoom.zoom(2, { animate: true })
    
    elem.parentElement.addEventListener('wheel', panzoom.zoomWithWheel)
    //elem.parentElement.addEventListener('click', panzoom.zoomIn)

*/
}

export function zoomOutelement(elementid) {
    /*
    const elem = document.getElementById(elementid)
    const panzoom = Panzoom(elem, {
        maxScale: 5
    })
    panzoom.pan(10, 10)
    panzoom.zoomOut()
    */

}

let fileHandle;
window.interop = {

    blazorDownloadFile: function (filename, contentType, content) {
        try {
            // Create a new Blob object from the content
            const blob = new Blob([content], { type: contentType });

            // Use the fetch API to download the file
            fetch(URL.createObjectURL(blob))
                .then(response => {
                    // Check if the response is successful
                    if (response.ok) {
                        // Create a new Blob object from the response
                        return response.blob();
                    }
                    throw new Error("Failed to download file");
                })
                .then(blob => {
                    // Create an object URL from the blob
                    const url = URL.createObjectURL(blob);

                    // Create a new <a> element and set its href attribute
                    const a = document.createElement("a");
                    a.href = url;

                    // Set the download attribute of the <a> element
                    a.download = filename;

                    // Append the <a> element to the document and click on it
                    document.body.appendChild(a);
                    a.click();

                    // Release the object URL
                    URL.revokeObjectURL(url);
                })
                .catch(error => {
                    console.error(error);
                });
        } catch (error) {
            console.error(error);
        }
    },


    getNewFileHandle: async function (suggestedFileName) {
        if (!fileHandle) {
            try {
                if (typeof window.chooseFileSystemEntries === 'function') {
                    // For Chrome 85 and earlier...
                    const opts = {
                        type: 'save-file',
                        accepts: [{
                            description: 'sNotes File',
                            extensions: ['snotes'],
                            mimeTypes: ['application/snotes'],
                        }],
                        suggestedName: suggestedFileName,
                    };
                    fileHandle = await window.chooseFileSystemEntries(opts);
                } else {
                    // handle the case where the function is not defined
                    // For Chrome 86 and later...
                    if ('showSaveFilePicker' in window) {
                        const opts = {
                            types: [{
                                description: 'sNotes File',
                                accept: { 'application/snotes': ['.snotes'] },
                            }],
                            suggestedName: suggestedFileName,
                        };
                        fileHandle = await window.showSaveFilePicker(opts);
                    }
                }
            } catch (e) {
                console.error('File picker was cancelled by the user');
            }
        } 

    },

    blazorSaveFile: async function (contentType, content) {

        if (fileHandle) {
            // Define the chunk size, e.g. 1 MB
            const chunkSize = 512 * 1024;
            // Calculate the number of chunks
            const numOfChunks = Math.ceil(content.length / chunkSize);

            // Create a FileSystemWritableFileStream to write to.
            const writable = await fileHandle.createWritable();

            // Write each chunk to the stream.
            for (let i = 0; i < numOfChunks; i++) {
                // Get the current chunk
                const start = i * chunkSize;
                const end = (i + 1) * chunkSize;
                const chunk = content.slice(start, end);

                // Create a new Blob object from the chunk
                const blob = new Blob([chunk], { type: contentType });

                // Write the contents of the chunk to the stream.
                await writable.write(blob);
            }

            // Close the file and write the contents to disk.
            await writable.close();
        }

    },


};




export function blazorDownloadFile2(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

export async function imgStreamToSrc(imageStream) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    return url;
}



export function registeGetProjectFileJSInstance(dotnetInstance) {
    window.GetProjectFileJSInstance = dotnetInstance;
}



export async function OpenSnotesFile() {
    const options = {
        types: [
            {
                description: 'sNotes File',
                accept: {
                    'application/snotes': ['.snotes']
                }
            },
        ],
        excludeAcceptAllOption: true,
        multiple: false
    };


    try {
        [fileHandle] = await window.showOpenFilePicker(options);
        const writable = await fileHandle.createWritable();
        window.GetProjectFileJSInstance.invokeMethod('SetLoaderValue', true);
        
        const file = await fileHandle.getFile();
        const arrayBuffer = await file.arrayBuffer();

        if (arrayBuffer.byteLength > 0) {
            const base64 = btoa(new Uint8Array(arrayBuffer).reduce((data, byte) => data + String.fromCharCode(byte), ''));
            updateFileArray(base64);
        } else {
            console.error("arrayBuffer is empty");
        }

        

    } catch (e) {
        
        console.error('File picker was cancelled by the user');
    }
}

 function updateFileArray(base64) {
     window.GetProjectFileJSInstance.invokeMethodAsync('UpdateFileArray', base64)
         .then(() => console.log("File sent."))
         .catch(error => console.error(error));

}



export function getBrowserName() {
    return navigator.userAgent;
}

