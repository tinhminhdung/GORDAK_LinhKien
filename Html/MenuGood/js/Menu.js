$(document).ready(function() 
{ 
	$(".Menu li").hover(function()
	{ 
	$(this).find('ul:first').css({visibility: "visible",display: "none"}).show(0); 
	},function(){ 
	$(this).find('ul:first').css({visibility: "hidden"}); 

	}); 
}); 