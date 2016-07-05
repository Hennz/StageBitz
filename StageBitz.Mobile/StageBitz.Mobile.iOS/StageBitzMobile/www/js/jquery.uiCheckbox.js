(function($){
    /*
        A.checkbox {
        }
        
        A.checkbox.checked {
        }
        
        ---
    
        $(':checkbox').uiCheckbox();
        $('input[type="checkbox"]').uiCheckbox();
        
        ---
        
        <a href="javascript:;" class="checkbox">&nbsp;</a>
        <a href="javascript:;" class="checkbox checked">&nbsp;</a>
        
        ---
        
        <input type="checkbox" name="x" value="1" checked="checked" />
        <input type="checkbox" name="y" value="1" />
    */

    jQuery.fn.uiCheckbox = function(o) {
        var defaults = {
            'tagName': 'a',
            'tagClassName': 'checkbox',
            'tagAttributes': 'href="javascript:;"',
            'tagContents': '&nbsp;',
            'checkedClassName': 'checked'
        };
        
        var options = $.extend({}, defaults, o);

        var hideCheckbox = function(el){
            if(!el){
                return;
            }
            
            el.css({ 'position': 'relative', 'left': -1000, 'opacity': 0 });
        };
        
        return this.each(function() {
            var el = $(this);
            var customTag = $('<' + options.tagName + ' ' + options.tagAttributes + (options.tagClassName != '' ? (' class="' + options.tagClassName + '"') : '') + '>' + options.tagContents + '</' + options.tagName + '>');        
            
            // Add custom tag.
            el.after(customTag);
            
            // Hide checkbox.
            hideCheckbox(el);
            
            // Set default state of the custom tag.
            if(el.is(':checked')){
                customTag.addClass(options.checkedClassName);
            }
            
            // Handle behavior of the custom tag.
            customTag.click(function(){
                customTag.toggleClass(options.checkedClassName);
                if(customTag.hasClass(options.checkedClassName)){
                    el.attr('checked', 'checked');
                } else {
                    el.removeAttr('checked');
                }
                return false;
            });
			   
            el.click(function(){
                customTag.toggleClass(options.checkedClassName);
				if(customTag.hasClass(options.checkedClassName)){
                    el.attr('checked', 'checked');
                } else {
                    el.removeAttr('checked');
                }
				return false;
            });
		
       });
    };
})(jQuery);