window.sendAjax = function (post_id, url, likeText, dislikeText, otherButton) {
    var user_id = $('#user-id').attr('data-id');  
    var data = {
        'postID': post_id,
        'userID': user_id
    };
    $.ajax({
        type: 'POST',
        url: url,
        data: data,
        success: function (res) {
            var text = data.split(',');
            console.log(text);
            likeText.text(text[0]);
            dislikeText.text(text[1]);
        },
        error: function (err) {
            console.log(err);
        }
    });
};

window.likePost = function (element) {
    var post_id = element.find('a').attr('id'); // Post id is stored as the div 
    post_id = post_id.substring(2, post_id.length);// Remove first two letters from ID

    var likeText = element.find('text');
    var button = element;

    var otherButton = element.siblings().find('span'); // The dislike button
    var dislikeText = otherButton.find('text');

    var url = '/Posts/Like/';
    if (otherButton.hasClass('active'))
        return dislikePost(otherButton);
    button.toggleClass('blue').toggleClass('active');

    sendAjax(post_id, url, likeText, dislikeText, otherButton);
};

window.dislikePost = function (element) {
    var post_id = element.find('a').attr('id'); // Post id is stored as the div 
    post_id = post_id.substring(2, post_id.length);// Remove first two letters from ID

    var dislikeText = element.find('text');
    var button = element;

    var otherButton = element.siblings().find('span'); // The like button
    var likeText = otherButton.find('text');

    var url = '/post/' + post_id + '/dislike';
    if (otherButton.hasClass('active'))
        return likePost(otherButton);
    button.toggleClass('red').toggleClass('active');

    sendAjax(url, likeText, dislikeText, otherButton);
};

$('.likes').on('click', function () {
    likePost($(this));

});
$('.dislikes').on('click', function () {
    dislikePost($(this));
});