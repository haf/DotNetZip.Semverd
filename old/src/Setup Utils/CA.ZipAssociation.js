// CA.ZipAssociation.js
//
// Store and Reset file associations for .zip files, as necessary, when
// DotNetZip is being installed and uninstalled, respectively.  This
// script defines custom actions that are invoked as part of the MSI.
//
// Copyright (c) 2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code, like all of DotNetZip, is licensed under the Microsoft
// Public License.  See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// Created: Thu, 14 Jul 2011  17:31
// Last saved: <2011-July-16 18:01:37>
//


/************************************************/
/* Message level                                */
/************************************************/
var msiMessageLevel = {
    FatalExit   : 0x00000000,
    Error       : 0x01000000,
    Warning     : 0x02000000,
    User        : 0x03000000,
    Info        : 0x04000000,
    ActionStart : 0x08000000,
    Progress    : 0x0A000000,
    ActionData  : 0x09000000
};


/************************************************/
/* Button styles                                */
/************************************************/
var msiButtonType = {
    Ok               : 0,
    OkCancel         : 1,
    AbortRetryIgnore : 2,
    YesNoCancel      : 3,
    YesNo            : 4,
    RetryCancel      : 5
};


/************************************************/
/* Default button                               */
/************************************************/
var msiDefaultButton = {
    First  : 0x000,
    Second : 0x100,
    Third  : 0x200
};

/************************************************/
/* Return values                                */
/************************************************/
var msiMessageStatus = {
    Error  : -1,
    None   : 0,
    Ok     : 1,
    Cancel : 2,
    Abort  : 3,
    Retry  : 4,
    Ignore : 5,
    Yes    : 6,
    No     : 7
};


var DotNetZipAssocId = "DotNetZip.zip.1";
var regPathZipAssoc = "HKEY_LOCAL_MACHINE\\SOFTWARE\\CLASSES\\.zip\\";
var regPathDnzPrior = "HKEY_CURRENT_USER\\SOFTWARE\\Dino Chiesa\\DotNetZip Tools v1.9\\PriorZipAssociation";
var verbose = true;

function DisplayMessageBox(message, options) {
    if (options === null) {
        options = msiMessageLevel.User + msiButtonType.Ok + msiDefaultButton.First;
    }

    if (typeof(Session) === undefined) {
        WScript.Echo(message);
        if ((options & 0xF) == 1) {
            // ask: cancel?
        }
        return 0;
    }

    var record = Session.Installer.CreateRecord(1);
    record.StringData(0) = "[1]";
    record.StringData(1) = message;

    return Session.Message(options, record);
}


function DisplayUserDiagnostic(message){
    if (!verbose) return 0;

    if (typeof(Session) === undefined) {
        WScript.Echo(message);
        return 0;
    }

    var options = msiMessageLevel.User + msiButtonType.Ok + msiDefaultButton.First;
    var record = Session.Installer.CreateRecord(1);
    record.StringData(0) = "[1]";
    record.StringData(1) = message;

    return Session.Message(options, record);
}

function LogMessage(msg) {
    var record = Session.Installer.CreateRecord(0);
    record.StringData(0) = "CustomAction: " + msg;
    Session.Message(msiMessageLevel.Info, record);
}


function mytrace(arg){
    if (verbose == false) return;
    // This just causes a regRead to be logged.
    // Then in PerfMon or RegMon, you can use it as a "trace"
    try {
        var junkTest = WSHShell.RegRead(regValue2 + arg);
    }
    catch (e2b) {
    }
}




function RestoreZipAssocInRegistry_CA() {
    // restore the app association for zip files, if possible.
    var WSHShell = new ActiveXObject("WScript.Shell");
    var priorAssociation = null;
    var phase = "";
    var currentAssociation;
    var stillInstalled;
    var parkingLot = "__DeleteThis";

    try {
        currentAssociation = WSHShell.RegRead(regPathZipAssoc);
        LogMessage("Current assoc for .zip: " + currentAssociation);

        if (currentAssociation == DotNetZipAssocId)
            phase = "1";
        else if (currentAssociation == "")
            phase = "2";

        LogMessage("phase " + phase);

        if (phase == "1" || phase=="2") {
            if (phase=="1")
                priorAssociation= WSHShell.RegRead(regPathDnzPrior);
            else
                priorAssociation= WSHShell.RegRead(regPathZipAssoc + parkingLot);

            LogMessage("prior assoc for .zip: " + priorAssociation);
            if (priorAssociation != "") {
                mytrace("A"+phase);
                try {
                    mytrace("B"+phase);
                    stillInstalled = WSHShell.RegRead(regPathZipAssoc + "OpenWithProgIds\\" + priorAssociation);
                    // the value will be the empty string
                    LogMessage("the prior app is still installed.");
                    mytrace("C");
                    if (phase=="1")
                        WSHShell.RegWrite(regPathZipAssoc + parkingLot, priorAssociation );
                    else {
                        WSHShell.RegWrite(regPathZipAssoc, priorAssociation );
                        WSHShell.RegDelete(regPathZipAssoc + parkingLot);
                    }
                }
                catch (e2a) {
                    mytrace("F");
                    LogMessage("the prior app is NOT still installed.");
                    WSHShell.RegWrite(regPathZipAssoc, "CompressedFolder");
                }
            }
            else {
                mytrace("G");
                LogMessage("the prior assoc is empty?");
                WSHShell.RegWrite(regPathZipAssoc, "CompressedFolder");
            }
        }
        else {
            LogMessage("the associated app has changed.");
            // The association has been changed since install of DotNetZip.
            // We won't try to reset it.
        }
    }
    catch (e1) {
        LogMessage("there is no associated app.");
        WSHShell.RegWrite(regPathZipAssoc, "CompressedFolder");
    }
}




function PreserveZipAssocInRegistry_CA() {
    // get and store the existing association for zip files, if any
    LogMessage("Hello from PreserveZipAssocInRegistry_CA()");
    var WSHShell = new ActiveXObject("WScript.Shell");
    var wantZipAssociation = Session.Property("WANT_ZIP_ASSOCIATIONS");
    LogMessage("wantZipAssociation = " + wantZipAssociation);

    if (wantZipAssociation == "1") {
        try {
            var association = WSHShell.RegRead(regPathZipAssoc);
            if (association != "") {
                LogMessage("PreserveFileAssoc: Current assoc for .zip: " + association);
                if (association != DotNetZipAssocId) {
                    LogMessage("PreserveFileAssoc: it is NOT DotNetZip.");
                    // there is an association, and it is not DotNetZip
                    WSHShell.RegWrite(regPathDnzPrior, association);
                }
                else {
                    LogMessage("PreserveFileAssoc: it is DotNetZip.");
                    // the existing association is for DotNetZip
                    try {
                        var priorAssoc = WSHShell.RegRead(regPathDnzPrior);
                        if (priorAssoc == "" || priorAssoc == DotNetZipAssocId) {
                            LogMessage("PreserveFileAssoc: defaulting (0)");
                            WSHShell.RegWrite(regPathDnzPrior, "CompressedFolder");
                        }
                        else {
                            // there already is a stored prior association.
                            // don't change it.
                        }
                    }
                    catch (e1a) {
                        LogMessage("PreserveFileAssoc: exception: " + e1a.message);
                        LogMessage("PreserveFileAssoc: defaulting (1)");
                        WSHShell.RegWrite(regPathDnzPrior, "CompressedFolder");
                    }
                }
            }
            else {
                // there is no default association for .zip files
                WSHShell.RegWrite(regPathDnzPrior, "CompressedFolder");
            }
        }
        catch (e1) {
            // the key doesn't exist (no app for .zip files at all)
            WSHShell.RegWrite(regPathDnzPrior, "CompressedFolder");
        }
    }
}



// var parameters = Session.Property("CustomActionData").split("|");
// var targetDir = parameters[0];
// var checkBoxState = parameters[1];

// DisplayDiagnostic("Checkbox state; " +  checkBoxState);
//
// PreserveFileAssociation();
// DeleteSelf();
//
//
// RestoreRegistry();
