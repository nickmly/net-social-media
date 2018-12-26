window.sendAjax = function (url, likeText, dislikeText, otherButton) {
    var user_id = $(".user-id").attr('id');
    $.ajax({
        type: 'GET',
        url: url,
        data: {
            user_id: user_id
        },
        success: function (data) {
            var text = data.split(",");
            console.log(text);
            likeText.text(text[0]);
            dislikeText.text(text[1]);
        }
    });
};

window.likePost = function (element) {
    var post_id = element.find("a").attr('id'); // Post id is stored as the div 
    post_id = post_id.substring(1, post_id.length);// Remove first letter from ID

    var likeText = element.find("text");
    var button = element;

    var otherButton = element.siblings().find("span"); // The dislike button
    var dislikeText = otherButton.find("text");

    var url = "/post/" + post_id + "/like";
    if (otherButton.hasClass("active"))
        return dislikePost(otherButton);
    button.toggleClass("blue").toggleClass("active");

    sendAjax(url, likeText, dislikeText, otherButton);
};

window.dislikePost = function (element) {
    var post_id = element.find("a").attr('id'); // Post id is stored as the div 
    post_id = post_id.substring(1, post_id.length);// Remove first letter from ID

    var dislikeText = element.find("text");
    var button = element;

    var otherButton = element.siblings().find("span"); // The like button
    var likeText = otherButton.find("text");

    var url = "/post/" + post_id + "/dislike";
    if (otherButton.hasClass("active"))
        return likePost(otherButton);
    button.toggleClass("red").toggleClass("active");

    sendAjax(url, likeText, dislikeText, otherButton);
};

$(".likes").on("click", function () {
    likePost($(this));

});
$(".dislikes").on("click", function () {
    dislikePost($(this));
});