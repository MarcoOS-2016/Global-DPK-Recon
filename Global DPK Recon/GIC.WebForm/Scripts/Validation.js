function show(divid) {
if (document.getElementById(divid).style.display != "")
    document.getElementById(divid).style.display = "";
else
    document.getElementById(divid).style.display = "block";
}

function checkvalue() {
var selectedccn = document.getElementById("MainContent_ccn").value;
if (selectedccn.trim().length == 0) {
    alert("Please select a CCN");
    return false;
}

//        var serialkeynum = document.getElementById("MainContent_serialkeynumber").value;
//        if (serialkeynum.trim().length == 0) {
//            alert("Please key in a Serial Key Number");
//            return false;
//        }

//        var partnumber = document.getElementById("MainContent_partnumber").value;
//        if (partnumber.trim().length == 0) {
//            alert("Please key in a Part Number ");
//            return false;
//        }

//        var lkmfromstatus = document.getElementById("MainContent_lkmfromstatus").value;
//        if (lkmfromstatus.trim().length == 0) {
//            alert("Please select a LKM From Status");
//            return false;
//        }

//        var lkmtostatus = document.getElementById("MainContent_lkmtostatus").value;
//        if (lkmtostatus.trim().length == 0) {
//            alert("Please select a LKM To Status");
//            return false;
//        }

var fromdate = document.getElementById("MainContent_fromdate").value;
if (fromdate.trim().length == 0) {
    alert("Please select From Date");
    return false;
}

var todate = document.getElementById("MainContent_todate").value;
if (todate.trim().length == 0) {
    alert("Please select To Date");
    return false;
}

var interval = new Date(todate) - new Date(fromdate);
if (interval < 0) {
    alert("The To Date you selected is earlier than From Date");
    return false;
}

// document.getElementById("viewreport").submit();
// document.transactionhistory.submit();
}