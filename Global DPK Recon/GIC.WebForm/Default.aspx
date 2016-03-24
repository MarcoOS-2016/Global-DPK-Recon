<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" EnableEventValidation="false"
    CodeBehind="Default.aspx.cs" Inherits="GIC.WebForm._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
<script type="text/javascript" src="My97DatePicker/WdatePicker.js"></script>
 <script type="text/javascript">
    function show(divid) {
        if (document.getElementById(divid).style.display != "")
            document.getElementById(divid).style.display = "";
        else
            document.getElementById(divid).style.display = "block";
    }

    function checkvalue() {
        var selectedsource = document.getElementById("MainContent_source").value;
        if (selectedsource.trim().length == 0) {
            alert("Please select a Source");
            return false;
        }

        var selectedccn = document.getElementById("MainContent_ccn").value;
        if (selectedccn.trim().length == 0) {
            alert("Please select a CCN");
            return false;
        }

//        var fromdate = document.getElementById("MainContent_fromdate").value;
//        if (fromdate.trim().length == 0) {
//            alert("Please select From Date");
//            return false;
//        }

//        var todate = document.getElementById("MainContent_todate").value;
//        if (todate.trim().length == 0) {
//            alert("Please select To Date");
//            return false;
//        }

        var fromdate = document.getElementById("MainContent_fromdatepicker").value;
        if (fromdate.trim().length == 0) {
            alert("Please select From Date");
            return false;
        }

        var todate = document.getElementById("MainContent_todatepicker").value;
        if (todate.trim().length == 0) {
            alert("Please select To Date");
            return false;
        }

        var interval = new Date(todate) - new Date(fromdate);
        if (interval < 0) {
            alert("The To Date you selected is earlier than From Date");
            return false;
        }
    }
</script>

<form runat="server" id="transactionhistory" action="default.aspx" method="post">
    <%--</div>--%>
    <table border="0" cellspacing="10" width="100%">
        <tr valign="middle">
        <td>Source: *</td>
        <td>
            <asp:dropdownlist runat="server" name="source" id="source" Width="210px">
                <asp:ListItem value="" Text="" />
                <asp:ListItem value="LKM" Text="LKM" />
                <asp:ListItem value="Glovia" Text="Glovia" />
	        </asp:dropdownlist>
        </td>
        <td>CCN: *</td>
        <td>
	        <asp:dropdownlist runat="server" name="ccn" id="ccn" Width="210px">
                <asp:ListItem value="" Text="" />
                <asp:ListItem value="DAO" Text="DAO" />
                <asp:ListItem value="BRH" Text="BRH" />
                <asp:ListItem value="DGPM" Text="DGPM" />
                <asp:ListItem value="E10000" Text="E10000" />
                <asp:ListItem value="C10000" Text="C10000" />
                <asp:ListItem value="C40000" Text="C40000" />
                <asp:ListItem value="C60000" Text="C60000" />
                <asp:ListItem value="M10000" Text="M10000" />
                <asp:ListItem value="I10000" Text="I10000" />
	        </asp:dropdownlist>
        </td>
        <td align="right"><asp:Button id="viewreport" Text="View Report" runat="server" onclientclick="return checkvalue()" onclick="viewreport_Click" />        
        </tr>
        <tr valign="middle">       
        <td>Serial Key Number:</td>
        <td><asp:TextBox runat="server" name="serialkeynumber" id="serialkeynumber" Width="160px"/></td>
        <td>Part Number:</td>
        <td><asp:TextBox runat="server" name="partnumber" id="partnumber"/></td>
        <td align="right"><asp:Button id="exportreport" Text="Export Report" runat="server" onclientclick="return checkvalue()" OnClick="exportreport_Button_Click"/></td>
        </tr>

        <tr valign="middle">
        <td>From Status:</td>
        <td>
            <asp:dropdownlist runat="server" name="lkmfromstatus" id="fromstatus">
                <asp:ListItem value="" Text="" />
                <asp:ListItem value="ALLOCATED" Text="ALLOCATED" />
                <asp:ListItem value="BOUND" Text="BOUND" />
                <asp:ListItem value="PEND2BURN" Text="PEND2BURN" />
                <asp:ListItem value="PO_SWAP" Text="PO_SWAP" />
                <asp:ListItem value="PRE_ALLOCATED" Text="PRE_ALLOCATED" />
                <asp:ListItem value="STAGED_TO_RETURN" Text="STAGED_TO_RETURN" />
                <asp:ListItem value="STAGED_TO_UNALLOCATED" Text="STAGED_TO_UNALLOCATED" />
                <asp:ListItem value="UNALLOCATED" Text="UNALLOCATED" />
            </asp:dropdownlist>
         </td>
    
         <td>To Status:</td>
         <td>
            <asp:dropdownlist runat="server" name="lkmtostatus" id="tostatus">
                <asp:ListItem value="" Text="" />
                <asp:ListItem value="ALLOCATED" Text="ALLOCATED" />
                <asp:ListItem value="ASSOCIATED" Text="ASSOCIATED" />
                <asp:ListItem value="BOUND" Text="BOUND" />
                <asp:ListItem value="PO_SWAP" Text="PO_SWAP" />            
                <asp:ListItem value="STAGED_TO_RETURN" Text="STAGED_TO_RETURN" />
                <asp:ListItem value="UNALLOCATED" Text="UNALLOCATED" />
            </asp:dropdownlist>
        </td>
        </tr>

        <%--<tr valign="middle">
        <td>From Date:</td>
        <td>
            <asp:TextBox runat="server" name="fromdate" id="fromdate" />
            <img src="image\calendar.jpg" alt="calendar" width="25" height="25" align="middle" onclick="show('fromdatecalendardiv');"/>
            <div id="fromdatecalendardiv" style="display:none; position:absolute; z-index:9999">
                <asp:Calendar id="fromdatecalendar" backcolor="White" runat="server" 
                    onselectionchanged="fromdatecalendar_SelectionChanged" />
            </div>
        </td>
        
        <td>To Date：</td>
        <td>
            <asp:TextBox runat="server" name="todate" id="todate"/>
            <img src="image\calendar.jpg" alt="calendar" width="25" height="25" align="middle" onclick="show('todatecalendardiv');"/>
            <div id="todatecalendardiv" style="display:none; position:absolute; z-index:9999">
                <asp:Calendar id="todatecalendar" backcolor="White" runat="server"
                    onselectionchanged="todatecalendar_SelectionChanged" Width="204px"/>
            </div>
        </td>
        </tr>--%>

        <tr valign="middle">
        <td>From Date: *</td>
        <td><input id="fromdatepicker" type="text" runat="server" class="Wdate" onfocus="WdatePicker({skin:'whyGreen',dateFmt:'yyyy-MM-dd HH:mm:ss',minDate:'2015-01-01 00:00:00',maxDate:'2028-12-31 23:59:59'})" /></td>
        <td>To Date: *</td>
        <td><input id="todatepicker" type="text" runat="server" class="Wdate" onfocus="WdatePicker({skin:'whyGreen',dateFmt:'yyyy-MM-dd HH:mm:ss',minDate:'2015-01-01 00:00:00',maxDate:'2028-12-31 23:59:59'})" /></td>
        </tr>
    </table>
    </form>
    <%--</div>--%>
    <div style="position:relative; overflow-y:auto; overflow-x:auto; top:30px; width=100%; height:400px">
    <asp:DataGrid runat="server" name="dataview" id="dataview" 
            AutoGenerateColumns="true" ondatabinding="dataview_DataBinding" alternatingItemColors="[0x0000ff,0x00ff00,0xff0000]" horizontalCenter="0" verticalCenter="0"/>
    </div>
</asp:Content>
