/// <reference path="../../../../node_modules/@types/jquery/index.d.ts" />

/// <reference path="../../../../node_modules/@types/angular/index.d.ts" />
/// <reference path="slickgrid/index.d.ts" />

declare namespace Q {
    function angularDefine(handler: (mdl: angular.IModule) => void);
    interface IUrlInfo {
        host: string;
        /**Absolue url including protocol and host */
        absUrl: IUrlInfoObject;
        /**Relative url exludeing protocol and host */
        relUrl: IUrlInfoObject;
    }
    interface IUrlInfoObject {
        /**not include page name */
        ref: string;
        /**inlcuding page name */
        value: string;
    }
    interface IQScope extends angular.IScope {
        /**Info of url after scope combine with an HTML layout */
        $$urlInfo: IUrlInfo;
        $doClose: () => void;
        $$ui: any;
        $ajax: any;
        $element: JQuery;
        $findEle: (selector: string, callback: (ele: JQuery) => void) => void
        $getUi<T>(id): T;
        $getViewId(): string;
        $getView<T>(): T;
    }

    interface ICallbackServiceCaller {
        done: any;
        url: string;
        id: string;
        data: any;
        method: string;
        noMask: boolean;
        emit: (err: any, res: any) => void;
    }

    interface IHistoryObject<T> {
        change: <T>(handler: (data: T) => void) => void;
        data: <T>() => T | any;
        onChange: <T>(scope: angular.IScope, handler: (data: T) => void) => void;
        redirectTo: (url: string) => void;
    }
    /**
     * Web API caller instance*/
    interface IWebServiceCallbackConfigCaller {
        call: (serverId: string) => IWebServiceCallbackConfigCaller;
        data: (data: any) => IWebServiceCallbackConfigCaller;
        done: (callback: (error: any, data: any) => void) => void;
        getCall: () => string;
        method: (value: string) => IWebServiceCallbackConfigCaller;
        setHeader: (key: string, value: any) => IWebServiceCallbackConfigCaller;
        url: (value: string) => IWebServiceCallbackConfigCaller;
    }

    /**Url instance manager */
    interface IUrlObject {
        /** add new or replace key with value if value is undefined the method will remove key from url*/
        param: (key, value) => IUrlObject;
        /**  add new or replace key with value if value is undefined the method will remove key from url*/
        def: (key, value) => IUrlObject;
        /** remove key*/
        remove: (key) => IUrlObject;
        /** remove key*/
        rem: (key) => IUrlObject;
        /** remove all keys*/
        clear: () => IUrlObject;
        /** remove all keys*/
        cls: () => IUrlObject;
        /** get bookmark url*/
        url: () => string;
        /**Jump to bookmark url */
        go: () => string;
        /** get bookmark url*/
        href: () => string;
        /**Jump to bookmark url */
        apply: () => void;
        /** get bookmark url*/
        toString: () => string;
        /** Get current url of page and unescape */
        uri: () => string;
    }
    /**Dialog */
    export interface IDialogObject {
        /**Set url .For relavte path will from $$urlInfo.absUrl.ref*/
        url: (path: string) => IDialogObject;
        /**Set param (send params to dialog) */
        params: <T>(data: T) => IDialogObject;
        //params:(data: any) => IDialogObject;
        /**On dialog close including manual or DOM destroy */
        onClose: (callback: (scope: IQScope) => void) => IDialogObject;
        /**Compile dailog and show */
        done: (callback?: (scope: IQScope) => void) => void;
        /**Set minimun size */
        size: (width, height?) => IDialogObject;
    }
    /**File reader event sender */
    interface IFileReaderSender {
        target: services.IFileReader,
        /**Inform to uploader if error uploader will be stop */
        emit: (error?: any) => void;
    }
    interface IFileReadingSender {
        /**Stop without reason */
        stop(): any;
        message: any;
        target: services.IFileReader,
        numOfChunks: number;
        length: number;
        readingSize: number;
        index: number;
        buffer: any;
        percent: number;
        /**Inform to uploader if error uploader will be stop */
        emit: (error?: any) => void;
    }
    namespace services {
        interface IFindScope {
            <T>(scopeIdOrScope: string | angular.IScope): T
        }
        /**Hook key press to any DOM element */
        interface IHookKeyPress {
            (eleObj: JQuery, callback: (keyCode: number, hots?: { isCtrl?: boolean, isShift?: boolean, isAlter?: boolean }) => boolean)
        }
        interface IUI {
            /**
             * Create copntex menu
             * @param target
             * @param pupupMenu
             * @param onHide
             */
            createContextMenu(target: Element, pupupMenu: Element, onHide: () => void): any;
            /**
             * get absolute position of element*/
            getPosition: (ele: Element) => { x: number, y: number };

        }
        interface ICallback {
            onCall: (handler: (sender: ICallbackServiceCaller) => void) => void;
        }
        /**
         * Web API caller configuration*/
        interface IWebServiceCallbackConfig {

            getCaller: (scope: IQScope) => IWebServiceCallbackConfigCaller;
            onBeforeSend: (value: any) => IWebServiceCallbackConfig;
            onBeforeCall: (handler: (sender: IWebServiceCallbackConfigCaller) => void) => IWebServiceCallbackConfig;
            /**
             * whenever call to server error
             * */
            onError: (sender: any) => IWebServiceCallbackConfig;
            onAfterCall: (handler: (sender: IWebServiceCallbackConfigCaller) => void) => IWebServiceCallbackConfig;
            onValidateResponse: (handler: (data: any, callback: (err: any, res: any) => void) => void) => IWebServiceCallbackConfig;
        }

        interface ICookie {
            create: (key, value, days) => void;
            getValue: (key) => any;
            remove: (key) => void;
        }
        /**
         * Web API caller version 2*/
        interface IPost {
            (
                scope: angular.IScope,
                serverMethod: string,
                postData: any,
                callback: (err: any, res: any) => void,
                noMkask?: boolean)
        }
        /**Url manager */
        interface IUrl {
            (): IUrlObject;
        }
        interface IDialog {
            (scope: angular.IScope): IDialogObject;
            (scope: Q.IQScope): IDialogObject;
            (scopeId: string): IDialogObject;
        }
        interface IMsg {
            success: (text) => void;
            error: (text) => void;
            info: (text) => void;
        }
        interface IFileReader {
            fileName: any;
            strFileSize: string;
            setChunkSize: (value: number) => void;
            humanFileSize: (bytes, si) => void;
            onReadFile: (handler: (sender: IFileReaderSender) => void) => void;
            onFinish: (sender: IFileReader) => void;
            onReading: (handler: (sender: IFileReadingSender) => void) => void;
            onError: (handler: (error: any) => void) => void;
            readFile: (event: Event) => void;
            emit: (error) => void;
        }
        interface IExt {
            apply<T>(scope: any, x: T): T;
            cast<T>(scope: any): T;
        }
        interface INav {
            change: <T>(handler: (data: T) => void) => void;
            data: <T>() => T;
            onChange: <T>(scope: angular.IScope, handler: (data: T) => void) => void;
            redirectTo: (url: string) => void;
        }
        /**Extend an Angular Scope  */
        interface IExtend {
            <T, T1>(obj: T1, type: T): T & T1
        }
        /**Cast an object to new with more mthehod and property has declared in T */
        interface ICast {
            <T, T1>(obj: T1, type: T): T & T1
        }
        /**Cast an object to new with more mthehod and property has declared in T */
        interface IExpand {
            /*
             * Mở rộng cấu trúc*
             */
            <T>(obj: T): <T1>() => T & T1
        }
        interface ITextFormat {
            (...args): string
        }
        interface IUtils {
            $wsConfig: IWebServiceCallbackConfig;
            $post: IPost;
            $url: IUrl;
            $dialog: IDialog;
            $msg: IMsg;
            $fileReader: IFileReader;
            $nav: INav;
            /**Cast an object to new with more mthehod and property has declared in T */
            $extCast: IExpand;
            /**Extend an Angular Scope */
            $extend: IExtend;
            $cast: ICast;
            $hookKeyPress: IHookKeyPress,
            $textFormat: ITextFormat;
        }
        namespace utils {
            interface IComponent {
                assign(id, scope, cmp)
            }
        }
    }
    namespace select2 {
        interface ISearchSender {
            searchValue: string;
            done: (data: { results: any }) => void
        }
    }
    namespace drectives {
        export interface sgCheckBoxSelect {

        }
        export interface sgCell {

        }
        export interface sGrid {


            onRequestData: string;
            /**
             * Fire on one item in grid click on enter
             * */
            onSelectItem: string;
            onKeyDown: string;
            /**
             * Data source list
             * */
            dataSource: string;
            /**
             * Serve for multi select rows grid with first check column
             * Examples:<s-grid><columns><column><sg-check-box-select data-field="<data-field>"/></column></columns></s-grid>
             * */
            dataKeyField: string;
            /**
             * Serve for multi select rows grid with first check column
             *  Examples:<s-grid><columns><column><sg-check-box-select data-field="<data-field>"/></column></columns></s-grid>
             * */
            dataSeletedItems: string;


        }
    }
    namespace ui {
        export interface IQSlickGrid {
            expandAll(level?: number): any;
            collapseAll(level?: number): any;
            addToParent(data, parentID): any;
            expandNodeById(id): any;
            selectedItem: any;
            key: string;
            parentKey: string;
            dataView: Slick.Data.DataView<any>;
            grid: Slick.Grid<any>;
            sortCols: Array<{ name: string; asc: boolean }>;
            search: (txtSearch: string, colsSearch?: string[]) => void;
            findById: <T>(id) => T;
            /**
             * Get all selected items 
             * if columns has 'sg-check-box-select' column 
             * and data-key-field
             *  Examples:<s-grid><columns><column><sg-check-box-select data-field="<data-field>"/></column></columns></s-grid>
             * */
            getSeletctedItems: <T>() => T[];
        }
    }
}
declare namespace quicky {
    function cast<T>(obj: any): T;
    function extend<T, T1>(obj: T1, type: T): T & T1;
}
declare namespace qLocalize {
    function getDateFormat(): string;
    function setDateFormat(format: string): string;
    function setApp(name: string);
    function setLanguageCode(name: string);
    function getLanguageCode(): string;
    function onSyncData(cb: (sender: any) => void);
    function setTenancy(tenancy: string): void;
    function setHostDir(dir: string);
    export interface IRegLang {
        res: (key: string) => string
        appRes: (key: string) => string
        gRes: (key: string) => string
    }

    function apply<T, T1>(instance: T, regFn: (reg: IRegLang) => T1, cb?: (x: T & { $$lang: Partial<T1> }) => void): T & {
        $$lang: Partial<T1>
    }
    function register<T, T1, T2, T3>(instance: T, regData: {
        gRes?: T1,
        appRes: T2,
        res: T3
    }): T & {
        $$lang: Partial<T1>,
        $localize: {
            app: Partial<T2>,
            res: Partial<T3>,
            global: Partial<T1>,
            },
            $res: (value: string) => string
    }
}