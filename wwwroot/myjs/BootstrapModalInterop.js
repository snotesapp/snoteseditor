
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

export function blazorDownloadFile1(filename, contentType, content) {
    // Create the URL
    const file = new File([content], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();


    // We don't need to keep the object URL, let's release the memory
    // On older versions of Safari, it seems you need to comment this line...
    URL.revokeObjectURL(exportUrl);

}

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

