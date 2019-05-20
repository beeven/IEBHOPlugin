jQuery.support.cors = true;

function sayHello(){
	alert("hello");
}

function sendData() {
	var json = {};//总json对象
	var jsonArray = [];//table中数据对象
	var divlist = document.getElementsByTagName('div');//获取所有的div对象
	for(var i=0;i<divlist.length;i++){
		json[i]=divlist[i].innerText;
	}
	var tableObj = $("table")[0];
	for (var i = 0; i < tableObj.rows.length; i++) {	//遍历Table的所有Row
		var jsonTd = {};
		for (var j = 0; j < tableObj.rows[i].cells.length; j++) {	//遍历Row中的每一列
			jsonTd[j]=tableObj.rows[i].cells[j].innerText;
		}
		jsonArray[i]=jsonTd;
	}
	json["table"]=jsonArray;
	var url = "http://10.53.2.16:8060/test/dzgf/htmlDataSave";
	$.ajax({
		type:'POST',
		data:{"jsondata":JSON.stringify(json)},
		url:url,
		dataType:'JSON',
		success:function(data, textStatus, jqXHR){
			if(data==1){
				alert("数据传输成功!");
			}else{
				alert("数据传输失败!");
			}
		},
		error:function(XMLHttpRequest, textStatus, errorThrown){
			var status = XMLHttpRequest.status;
			var statusText = XMLHttpRequest.statusText;
			if(status==200){
				console.log("链接成功！");
			}
			console.error(errorThrown);
			console.error(textStatus);
		}
	});
}
