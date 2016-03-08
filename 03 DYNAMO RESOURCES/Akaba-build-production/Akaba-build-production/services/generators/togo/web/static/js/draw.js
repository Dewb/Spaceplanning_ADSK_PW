function change_grid()
{
    var xmlhttp = new XMLHttpRequest();
    var url = "http://localhost:5000/change";
    xmlhttp.onreadystatechange = draw_data_callback(xmlhttp);
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

function reset_grid()
{
    var xmlhttp = new XMLHttpRequest();
    var url = "http://localhost:5000/reset";
    xmlhttp.onreadystatechange = draw_data_callback(xmlhttp);
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

function step_grid()
{
    var xmlhttp = new XMLHttpRequest();
    var url = "http://localhost:5000/step";
    xmlhttp.onreadystatechange = draw_data_callback(xmlhttp);
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

function draw_data_callback(xmlhttp)
{
    return function()
    {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) 
        {
            draw(xmlhttp.response);
        }
    }
}

function pull_draw_data()
{
    var xmlhttp = new XMLHttpRequest();
    var url = "http://localhost:5000/grid.svg";

    xmlhttp.onreadystatechange = draw_data_callback(xmlhttp);
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

function draw(svg) 
{
    // Make an instance of two and place it on the page.
    var elem = document.getElementById('draw-shapes');
    elem.innerHTML = svg;
    _frame_index += 1;
}

var _auto_step = undefined;
var _frame_index = 0;
var _last_frame_index = 0;

function auto_step()
{
    if (_frame_index > _last_frame_index)
    {
        _last_frame_index = _frame_index;
        step_grid();
    }
}

function enable_refresh()
{
    if (_auto_step == undefined)
    {
        _auto_step = setInterval(auto_step, 50);
        step_grid();
    }
}

function disable_refresh()
{
    if (_auto_step != undefined)
    {
        clearInterval(_auto_step);
        _auto_step = undefined;
    }
}
