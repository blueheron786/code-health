window.blazorFolderPicker = {
    pickFolder: async function () {
        let folderPath = await window.showDirectoryPicker();
        return folderPath.name;
    }
}