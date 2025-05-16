function ConvertSpanFromUTC(spanid) {
    var d = new Date(0);
    d.setUTCSeconds(document.getElementById(spanid).innerHTML);
    document.getElementById(spanid).innerHTML = d.toLocaleString('en-CA', { dateStyle: 'short', timeStyle: 'short', timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone }).replace(':00', '').replace(',', '').replace('.', '').replace('.', '').toUpperCase();
}
