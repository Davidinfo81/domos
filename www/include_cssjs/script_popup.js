<!--//Michel DEBOOM 06/2005
gk=window.Event?1:0; // navigateurs Gecko ou IE
D=document;popup=encours=0

function ctrl(e)
{
de=!D.documentElement.clientWidth?D.body:D.documentElement // IE6
sx=gk?pageXOffset:de.scrollLeft //scroll h
sy=gk?pageYOffset:de.scrollTop //scroll v
x=gk?e.pageX:event.clientX+sx; //curseur x
y=gk?e.pageY:event.clientY+sy; //curseur y
el=gk?e.target:event.srcElement;
if(!el.tagName)el=el.parentNode; // noeud #text
if(el.className == 'pop')
  {
  popup = D.getElementById(el.href.substring(el.href.lastIndexOf('#') + 1)).style; 
  if(popup!=encours) // seulement si changement  
    {
    encours.display='none';
    with(popup){display="block";left=x+'px';top=y+10+'px';}
    encours=popup;
    }
  } else {encours.display='none';encours=0}
}

D.onmousemove=ctrl
// charge la feuille de style des popups.
D.write('<style type="text/css">@import url(popup.css);</style>')
//-->