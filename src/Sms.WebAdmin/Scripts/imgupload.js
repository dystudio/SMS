//默认为单图上传
var uploadType = 0;
//触发上传的按钮对象
var uploadObj = null;

$(function () {

    $("#imgUpload").ajaxForm(function (obj) {
        if (obj.Status) {
            var $athis = $(uploadObj);
            var $_moreImg = $athis.next(".thumbnails");
            var $_input = $athis.prevAll("input[type='hidden']");
            var newImg = '<li class="span2"> <a href="javascript:" title="点击放大"  class="thumbnail"> <img src="/Upload/Image/' + obj.Url + '" alt=""> </a> <div class="actions"> <a title="移除" href="javascript:" class="op"><i class="icon-remove icon-white"></i></a> </div> </li>';
            if (uploadType == 1) {//多图列表
                $_moreImg.append(newImg);
                var oldValue = $_input.val();//拿到旧值
                if (oldValue && oldValue != "") {
                    oldValue += "$" + obj.Url;
                }
                else {
                    oldValue += obj.Url;
                }
                //设值
                $_input.val(oldValue);
            }
            else if (uploadType == 0) {//单图
                $_moreImg.html(newImg);
                $_input.val(obj.Url);
            }
            BindEvent();
        }
        else {
            show_message(false, obj.MsgText, null);

        }
    });
    //初始化事件绑定
    BindEvent();
});

//绑定通用事件
function BindEvent() {
    //多图删除事件
    $(".thumbnails .op").unbind("click").click(function () {
        if (confirm("确认移除该图片吗？")) {
            var base = $(this).parents(".thumbnails");
            $(this).parents("li").remove();
            //删除隐藏值
            var $_input = base.prevAll("input[type='hidden']");
            var newValue = "";
            base.children("li").each(function (i) {
                var excision = i == 0 ? "" : "$";
                newValue += excision + $(this).find("img").attr("src").replace("/Upload/Image/", "");
            });
            $_input.val(newValue);
        }
    });

    openView();
}


