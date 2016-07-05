
function ConfigureUI(chkBoxobject) {
    //chkBoxobject.style.visibility = 'hidden';
    
    
    if (chkBoxobject.checked == true) {
        //Show the educational discount Line
        $(".divEducationalDiscount").show();
        $(".trPromotionalCode").hide();
        $(".lblDiscountToSummary").hide();
        $(".lblDiscountedAmountText").hide();
    }
    else {
        $(".divEducationalDiscount").hide();
        $(".trPromotionalCode").show();
        $(".lblDiscountToSummary").show();
        $(".lblDiscountedAmountText").show();
    }
    CalculateTotal(chkBoxobject);
}

function CalculateTotal(chkBoxobject) {
    //alert(document.getElementById("<%= hdnTotalAfterDiscount.ClientID %>"));
   
    var hdnTotalAfterDiscount = $(".hdnTotalAfterDiscount").html();
    var hdnTotalAfterEducationalPackage = $(".hdnTotalAfterEducationalPackage").html();
    if (chkBoxobject.checked == true) {
        $(".lblTotaltoPay").html(hdnTotalAfterEducationalPackage);
    }
    else {
        $(".lblTotaltoPay").html(hdnTotalAfterDiscount);
    }
}
