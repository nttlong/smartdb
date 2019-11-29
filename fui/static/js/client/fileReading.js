function FileObject() {

};
FileObject.prototype.setChunkSize = function (value) {
    this._chunkSize = value;
};
FileObject.prototype.humanFileSize = function (bytes, si) {
    var thresh = si ? 1000 : 1024;
    if (Math.abs(bytes) < thresh) {
        return bytes + ' B';
    }
    var units = si
        ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
        : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
    var u = -1;
    do {
        bytes /= thresh;
        ++u;
    } while (Math.abs(bytes) >= thresh && u < units.length - 1);
    return bytes.toFixed(1) + ' ' + units[u];
};
FileObject.prototype.onReadFile = function (callback) {
    this._onReadFile = callback;
};
FileObject.prototype.onFinish = function (callback) {
    this._onFinish = callback;
};
FileObject.prototype.onReading = function (callback) {
    this._onReading = callback;
};
FileObject.prototype.onError = function (callback) {
    this._onError = callback;
};
FileObject.prototype.readFile = function (event) {
    this.fileSize = event.target.files[0].size;
    this.strFileSize = this.humanFileSize(this.fileSize, "MB");
    this.fileName = event.target.files[0].name;
    
    var _file = event.target.files[0];
    var r = new FileReader();
    var _offset = 0;
    this._chunkSize = this._chunkSize | (3*1024);
    var length = this._chunkSize|1024;
    var numOfChunk = Math.floor(this.fileSize / this._chunkSize);
    var chunks = [];
    for (var i = 0; i < numOfChunk;i++) {
        chunks.push(this._chunkSize);
    }
    if (this.fileSize % this._chunkSize > 0) {
        chunks.push(this.fileSize - this._chunkSize * numOfChunk);
    }
    this.chunks = chunks;
    var me = this;
    var _sender = {
        target: this,
        emit: function (error) {
            if (error) {
                if (me._onError) {
                    me._onError(error);
                }
                else {
                    throw (error);
                }
                return;
            }
            function _arrayBufferToBase64(buffer) {
                var binary = '';
                var bytes = new Uint8Array(buffer);
                var len = bytes.byteLength;
                for (var i = 0; i < len; i++) {
                    binary += String.fromCharCode(bytes[i]);
                }
                return window.btoa(binary);
            }
            function chunkReaderBlock(off, index, file) {
                if (index < chunks.length) {
                    var blob = file.slice(off, off + chunks[index]);
                    r.onload = function (e) {
                        var buffer = _arrayBufferToBase64(e.target.result);
                        var sender = {
                            target: me,
                            numOfChunks: chunks.length,
                            length: me.fileSize,
                            readingSize: off + chunks[index],
                            index: index,
                            buffer: buffer,
                            percent: ((off + chunks[index])/me.fileSize)*100,
                            emit: function (error) {
                                if (!error) {
                                    chunkReaderBlock(off + chunks[index], index + 1, file);
                                }
                                else {
                                    if (me._onError) {
                                        me._onError(error);
                                    }
                                    else {
                                        throw (error);
                                    }
                                }
                            }
                        };
                        if (me._onReading) {
                            me._onReading(sender);
                        }
                        else {
                            chunkReaderBlock(off + chunks[index], index + 1, file);
                        }
                    };
                    r.readAsArrayBuffer(blob);
                }
                else {
                    if (me._onFinish) {
                        me._onFinish(me);
                    }
                }
            }

            chunkReaderBlock(0, 0, _file);
        }
    }
    if (this._onReadFile) {
        this._onReadFile(_sender);
    }
    
    
};
var fileObject = new FileObject();