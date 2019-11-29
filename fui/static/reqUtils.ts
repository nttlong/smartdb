/// <reference path="js/client/types/require.d.ts" />

class clsLibPath {
    loadCss(cssLinks: string[]): void {
        var me = this;
        cssLinks.forEach((item, index) => {
            var link = document.createElement("link");
            link.type = "text/css";
            link.rel = "stylesheet";
            link.href = `${me.rootPath}/${item}`;
            document.getElementsByTagName("head")[0].appendChild(link);
        });
        
    }
    load(libPath: string[]): clsLibPathThen {
       
        var ret = new clsLibPathThen();
        ret.rootUrl = this.rootPath;
        ret.load(libPath);
        return ret;
        
    }
    listOfPaths: string[];
    toArray(): string[] {
        return this.listOfPaths;
    }
    paths: string[];
    ref(paths: string[]): clsLibPath {
        this.paths = paths;
        this.listOfPaths = [];
        var me = this;
        this.paths.forEach((item, index, lst) => {
            this.listOfPaths.push(`${me.rootPath}/${item}`);
        });
        return this;
    }
    rootPath: string;
    constructor(rPath: string) {
        this.rootPath = rPath;
    }
}
class clsLibPathThen {
    rootUrl: string;
    constructor() {
        this.urls = [];
    }
    thenLoad(libs: string[]): clsLibPathThen {
        var lst = [];
        var me = this;
        libs.forEach((item, index) => {
            lst.push(`${me.rootUrl}/${item}`);
        });
        this.urls.push(lst)
        return this;
    }
    _handler: () => void;
    then(handler: () => void): clsLibPathThen {
        this._handler = handler;
        var me = this;
        function run(index, callback) {
            if (index >= me.urls.length) {
                callback();
            }
            else {
                requirejs(me.urls[index], () => {
                    run(index + 1, callback);
                });
            }
        }
        run(0, () => {
            handler();
        });
        return me;
    }
    urls: Array<string[]>;
    load(libs: string[]): any {

        var lst = [];
        var me = this;
        libs.forEach((item, index) => {
            lst.push(`${me.rootUrl}/${item}`);
        });
        this.urls.push(lst)
        return this;
        
    }
}
var reqUtils = {
    from: (strPath: string): clsLibPath=>
    {
        return new clsLibPath(strPath);
    }
}
