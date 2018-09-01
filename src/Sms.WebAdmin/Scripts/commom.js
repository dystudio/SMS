$(document).ready(function () {
    $('input[type=checkbox],input[type=radio],input[type=file]').uniform();

    $('.tree li:has(ul)').addClass('parent_li').find(' > span').attr('title', '收起');
    $('.tree li.parent_li > span').on('click', function (e) {
        var children = $(this).parent('li.parent_li').find(' > ul > li');
        if (children.is(":visible")) {
            children.hide('fast');
            $(this).attr('title', '展开').find(' > i').addClass('icon-plus-sign').removeClass('icon-minus-sign');
        } else {
            children.show('fast');
            $(this).attr('title', '收起').find(' > i').addClass('icon-minus-sign').removeClass('icon-plus-sign');
        }
        e.stopPropagation();
    });
    $("#btnToggle").bind("click", function () {
        var branch = $('.tree li.parent_li > span');
        if (branch.length > 0) {
            branch.click();
            if ($(this).text() == "全部展开") {
                $(this).text("全部收起");
            }
            else { $(this).text("全部展开"); }
        }
    });
    $("#btnCheckAll").bind("click", function () {
        var checkbox = $(".checkbox");
        if (checkbox.length > 0) {
            if ($(this).text() == "全选") {
                checkbox.prop("checked", "checked");
                checkbox.parent().addClass("checked");
                $(this).text("取消");
            }
            else {
                checkbox.removeAttr("checked");
                checkbox.parent().removeClass("checked");
                $(this).text("全选");
            }
        }
    });

    ///------------列表中的全选操作begin---------------------
    $("#title-table-checkbox-all").bind("click", function () {
        var child = $(".checkbox-item");
        if ($(this).is(':checked')) {
            $(this).prop("checked", "checked");
            if (child.length > 0) {
                child.prop("checked", "checked").parent().addClass("checked");
            }
        }
        else {
            $(this).removeAttr("checked");
            if (child.length > 0) {
                child.removeAttr("checked").parent().removeClass("checked");
            }
        }
    });

    $(".checkbox-item").bind("click", function () {
        var all = $(".checkbox-item").length;
        var checked = $(".checkbox-item:checked").length;
        var btnAll = $("#title-table-checkbox-all");
        if (all == checked) {
            btnAll.prop("checked", "checked").parent().addClass("checked");
        }
        else {
            btnAll.removeAttr("checked").parent().removeClass("checked");
        }
    })
    ///------------列表中的全选操作end---------------------

    openView();
});

function openView() {
    //缩略图放大事件
    $(".thumbnails .thumbnail").unbind("click").click(function () {
        var path = $(this).find("img").attr("src");
        if (path && path != '') {
            $("#BigView").find("img").attr("src", path);
            $("#modalbackground").addClass("in").show();
            $("#BigView").fadeIn(200);
            $("#BigView").animate({
                "top": $(document).scrollTop() + parseInt($("#BigView").css('top')) + 'px',
                "display": "block"
            }, 200, null);
        }
        else {
            show_message(false, '未找到图片！', null);
        }
    });
}

function closeView() {
    setTimeout(function () {
        $("#modalbackground").removeClass("in").hide();
    }, 250);
    $("#BigView").css("top", "50px").fadeOut(200);
}

//获取列表的选中项
function selectedCheckboxValue() {
    var idStr = '';
    $.each($(".checkbox-item:checked"), function (index, item) {
        if (idStr == '') {
            idStr += item.value;
        }
        else {
            idStr += ',' + item.value;
        }
    })
    return idStr;
}

//通用的Ajax操作封装
function AjaxFunction(method, url, param, done, fail) {
    $.ajax({
        type: method,
        url: url,
        data: param,
        dataType: "json",
        beforeSend: function () {
            OnSubmiting();
        },
        complete: function () {
            AjaxCompelement();
        },
        error: function (XMLHttpRequest, textStatus) {
            show_message(false, textStatus, null);
        },
        success: function (data) {
            if (data.Status===true) {
                if (done) done(data);
            }
            else {
                show_message(false, data.MsgText, data.Url);
                if (fail) fail(data);
            }
        }
    });
}

var show_message = function (result, msg, url) {
    var elem = $("#alertsuccess");
    if (!result) {
        elem = $("#alerterror");
    }
    elem.find("span").text(msg);
    $("#alertbox").show();
    elem.fadeTo(300, 1);
    setTimeout(function () {
        elem.fadeTo(2000, 0, function () { $("#alertbox").hide(); });
        if (url != undefined && url != null && url != '') {
            setTimeout(function () { location.href = url; }, 1200)
        }
    }, 1200);
}

var basic_validate = function (form, rulesObject, messagesObject) {
    $("#" + form).validate({
        rules: rulesObject,
        messages: messagesObject,
        errorClass: "help-inline",
        errorElement: "span",
        highlight: function (element, errorClass, validClass) {
            $(element).parents('.control-group').removeClass('success');
            $(element).parents('.control-group').addClass('error');
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).parents('.control-group').removeClass('error');
            $(element).parents('.control-group').addClass('success');
        }
    });
}

function ShowDialog() {
    $("#modalbackground,#myModal").addClass("in").show();
}

function CloseDialog() {
    $("#modalbackground,#myModal").removeClass("in").hide();
}

//异步执行前的处理
function OnSubmiting() {
    $("#loading").show();
}

//异步执行完后的通用回调函数
function AjaxCompelement() {
    $("#loading").hide();
}

