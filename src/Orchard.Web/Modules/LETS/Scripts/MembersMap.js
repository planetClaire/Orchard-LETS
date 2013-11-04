function initializeMap(latitude, longitude, zoomLevel) {
    var mapOptions = {
        center: new google.maps.LatLng(latitude, longitude),
        zoom: zoomLevel,
        maxZoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
    infoWindow = new window.google.maps.InfoWindow();
}

function AddMarker(objMarker) {

    var latlng = objMarker.LatLong.split(",");
    var position = new window.google.maps.LatLng(latlng[0], latlng[1]);
    var marker = new window.google.maps.Marker({
        position: position,
        map: map,
        title: "Please click me!"
    });
    window.google.maps.event.addListener(marker, 'click', function () {
        infoWindow.content = objMarker.InfoHtml;
        infoWindow.open(map, marker);
    });
}


