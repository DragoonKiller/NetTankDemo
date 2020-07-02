//
// 这个脚本扫描目录下的所有图像文件.
// 如果发现图像文件的大小大于 10M, 则读取该图片并生成一个 JPG.
// 支持的格式写在 allowedSuffix 中.
// 

let fs = require('fs')
let path = require('path')
let allowedSuffix = ['png', 'tiff']
let jimp = require('jimp')


let targetPath = process.argv[2]

let foreachFile = (rootPath, callback) => {
    fs.readdir(rootPath, (err, files) => {
        files.forEach((filename) => {
            let acutalPath = path.join(rootPath, filename)
            let stats = fs.lstatSync(acutalPath)
            let isFile = stats.isFile()
            let isDir = stats.isDirectory()
            if(isFile) {
                callback(acutalPath)
            } else if(isDir) {
                foreachFile(acutalPath, callback)
            }
        });
    });
}

foreachFile(targetPath, (filename) => {
    let isImage = allowedSuffix.reduce((a, validSuffix) => filename.endsWith(validSuffix) ? a || true : a, false)
    if(isImage) {
        let filenameNoExt = filename.split('.').slice(0, -1).join('.')
        console.log(filenameNoExt)
        
        jimp.read(filename)
        .then(img => {
            return img
                .quality(50)
                .write(filenameNoExt + '.jpg')
        })
        .catch(err => console.log(err))
    }
});
