
export function CloseModal(modalId) {
    $(modalId).modal('hide');

    
}


export function showPrompt(message) {
    return prompt(message, 'Type anything here');
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

