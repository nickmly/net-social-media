const cardOpeners = document.getElementsByClassName("card-opener");
for(var i = 0; i < cardOpeners.length; i ++) {
    (function(index){
        cardOpeners[index].addEventListener("click", function() {            
            const cardBody = document.getElementById("card-body-" + cardOpeners[index].getAttribute("data-id"));
            var visible = cardBody.style.display == "block"
            cardOpeners[index].querySelector(".fas").classList.toggle("fa-caret-down");
            cardOpeners[index].querySelector(".fas").classList.toggle("fa-caret-up");
            if(visible) {
                cardBody.style.display = cardBody.style.display = "none";
                cardOpeners[index].querySelector(".card-opener-text").innerHTML = "View More";
            } else {
                cardBody.style.display = "block";
                cardOpeners[index].querySelector(".card-opener-text").innerHTML = "Close";
            }
        });
    })(i);
}