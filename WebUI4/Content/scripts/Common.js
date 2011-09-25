// Dependent on JQuery


// Declare Global Variables to keep application-wide javascript settings.
jQuery.appSettings = {
    enableLog: true,
    enableAlert: false
    };

// Implement logging function.
jQuery.log = function(message) {
    if ($.appSettings.enableLog == true) {
        if (window.console) {
            console.debug('Application Debug: ' + message);
        }
        if ($.appSettings.enableAlert == true) {
            alert('Application Debug: ' + message);
        }
    }
};

/* Extend jQuery with functions for PUT and DELETE requests. */
function _ajax_request(url, data, callback, type, method) {
    if (jQuery.isFunction(data)) {
        callback = data;
        data = {};
    }
    return jQuery.ajax({
        type: method,
        url: url,
        data: data,
        success: callback,
        dataType: type
    });
}

jQuery.extend({
    put: function(url, data, callback, type) {
        return _ajax_request(url, data, callback, type, 'PUT');
    },
    delete_: function(url, data, callback, type) {
        return _ajax_request(url, data, callback, type, 'DELETE');
    }
});


////  Text Utilities:

accentsTidy = function (s) {
    var r = s.toLowerCase();
    r = r.replace(new RegExp("\\s", 'g'), "");
    r = r.replace(new RegExp("[àáâãäå]", 'g'), "a");
    r = r.replace(new RegExp("æ", 'g'), "ae");
    r = r.replace(new RegExp("ç", 'g'), "c");
    r = r.replace(new RegExp("[èéêë]", 'g'), "e");
    r = r.replace(new RegExp("[ìíîï]", 'g'), "i");
    r = r.replace(new RegExp("ñ", 'g'), "n");
    r = r.replace(new RegExp("[òóôõö]", 'g'), "o");
    r = r.replace(new RegExp("œ", 'g'), "oe");
    r = r.replace(new RegExp("[ùúûü]", 'g'), "u");
    r = r.replace(new RegExp("[ýÿ]", 'g'), "y");
    //r = r.replace(new RegExp("\\W", 'g'), "");
    return r;
};




/* Serializes the jQuery object into a json object.  */

$.fn.serializeForm = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};


function escapeSelector(str) {
    if(str)
        return str.replace(/([ #;&,.+*~\':"!^$[\]()=>|\/@])/g,'\\$1')
    else
        return str;
}







$(document).ready(function () {
    // Sets the focus on the first control.
    $(':input:not([readonly]):first').focus();
});