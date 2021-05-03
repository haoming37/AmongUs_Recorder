function httpGet(theUrl)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open( "GET", theUrl, false ); // false for synchronous request
    xmlHttp.send( null );
    return xmlHttp.responseText;
}

function httpGetAsync(theUrl, callback)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function() { 
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    }
    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.send(null);
}

var table = new Vue({
    el: '#app',
    data :{
        games: []
    },
    methods: {
        async getGames() {
            var url = 'http://localhost:8000/recorder/games'
            httpGetAsync(url, x=>{this.games = x})
        }
    },
    mounted() {
        this.getGames()
    }
})