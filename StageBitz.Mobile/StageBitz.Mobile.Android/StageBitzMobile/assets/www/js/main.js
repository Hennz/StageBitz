$(document).ready(function(){
	$('.auto-hint').focus(function(){
		if ($(this).attr('value') == $(this).attr('title')) $(this).attr('value', '');
	});
	$('.auto-hint').blur(function(){
		if ($(this).attr('value') == '') $(this).attr('value', $(this).attr('title'));
	});
	
	$('select').selectbox();	
		
	
	$(document).bgStretcher({
		images: ['css/images/bg/body-bg.jpg'],
		imageWidth: 1278, imageHeight: 1831, slideShow: false
	});
	if ( $(":checkbox").length > 0 ) {
		$(':checkbox').uiCheckbox();
	}
   
});