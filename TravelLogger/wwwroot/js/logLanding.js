
$(document).ready(function() {
    function sendRequest(url, method, data) {
        $.LoadingOverlay("show");
        var reload =  function() { window.location.reload(); };
        $.ajax({
            url: url,
            method: method,
            data: data,
            success: reload,
            error: reload
        });
    }

    function handleDiscard(event) {
        sendRequest("/Log/Discard", "DELETE");
    }

    function handleDeparture(event) {
        sendRequest("/Log/Depart", "PUT");
    }

    function handleArrival(event) {
        var data = {};
        var comment = $("#comment").val().trim();
        if (comment.length > 0) data.comment = comment;
        sendRequest("/Log/Arrive", "POST", data);
    }

    $("#discard").click(handleDiscard);
    $("#arrival").click(handleArrival);
    $("#departure").click(handleDeparture);
});
