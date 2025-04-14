import {
  require_react_dom
} from "./chunk-4BWY72AQ.js";
import {
  require_dist,
  require_set_cookie
} from "./chunk-YE47WNNT.js";
import {
  require_react
} from "./chunk-XSOSCO6V.js";
import {
  __commonJS,
  __esm,
  __export,
  __publicField,
  __reExport,
  __toCommonJS,
  __toESM
} from "./chunk-FOQIPI7F.js";

// node_modules/@babel/runtime/helpers/interopRequireDefault.js
var require_interopRequireDefault = __commonJS({
  "node_modules/@babel/runtime/helpers/interopRequireDefault.js"(exports, module) {
    function _interopRequireDefault(e) {
      return e && e.__esModule ? e : {
        "default": e
      };
    }
    module.exports = _interopRequireDefault, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/typeof.js
var require_typeof = __commonJS({
  "node_modules/@babel/runtime/helpers/typeof.js"(exports, module) {
    function _typeof2(o) {
      "@babel/helpers - typeof";
      return module.exports = _typeof2 = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(o2) {
        return typeof o2;
      } : function(o2) {
        return o2 && "function" == typeof Symbol && o2.constructor === Symbol && o2 !== Symbol.prototype ? "symbol" : typeof o2;
      }, module.exports.__esModule = true, module.exports["default"] = module.exports, _typeof2(o);
    }
    module.exports = _typeof2, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/regeneratorRuntime.js
var require_regeneratorRuntime = __commonJS({
  "node_modules/@babel/runtime/helpers/regeneratorRuntime.js"(exports, module) {
    var _typeof2 = require_typeof()["default"];
    function _regeneratorRuntime() {
      "use strict";
      module.exports = _regeneratorRuntime = function _regeneratorRuntime2() {
        return e;
      }, module.exports.__esModule = true, module.exports["default"] = module.exports;
      var t, e = {}, r = Object.prototype, n = r.hasOwnProperty, o = Object.defineProperty || function(t2, e2, r2) {
        t2[e2] = r2.value;
      }, i = "function" == typeof Symbol ? Symbol : {}, a = i.iterator || "@@iterator", c = i.asyncIterator || "@@asyncIterator", u = i.toStringTag || "@@toStringTag";
      function define(t2, e2, r2) {
        return Object.defineProperty(t2, e2, {
          value: r2,
          enumerable: true,
          configurable: true,
          writable: true
        }), t2[e2];
      }
      try {
        define({}, "");
      } catch (t2) {
        define = function define2(t3, e2, r2) {
          return t3[e2] = r2;
        };
      }
      function wrap(t2, e2, r2, n2) {
        var i2 = e2 && e2.prototype instanceof Generator ? e2 : Generator, a2 = Object.create(i2.prototype), c2 = new Context(n2 || []);
        return o(a2, "_invoke", {
          value: makeInvokeMethod(t2, r2, c2)
        }), a2;
      }
      function tryCatch(t2, e2, r2) {
        try {
          return {
            type: "normal",
            arg: t2.call(e2, r2)
          };
        } catch (t3) {
          return {
            type: "throw",
            arg: t3
          };
        }
      }
      e.wrap = wrap;
      var h = "suspendedStart", l = "suspendedYield", f = "executing", s = "completed", y = {};
      function Generator() {
      }
      function GeneratorFunction() {
      }
      function GeneratorFunctionPrototype() {
      }
      var p = {};
      define(p, a, function() {
        return this;
      });
      var d = Object.getPrototypeOf, v = d && d(d(values([])));
      v && v !== r && n.call(v, a) && (p = v);
      var g = GeneratorFunctionPrototype.prototype = Generator.prototype = Object.create(p);
      function defineIteratorMethods(t2) {
        ["next", "throw", "return"].forEach(function(e2) {
          define(t2, e2, function(t3) {
            return this._invoke(e2, t3);
          });
        });
      }
      function AsyncIterator(t2, e2) {
        function invoke(r3, o2, i2, a2) {
          var c2 = tryCatch(t2[r3], t2, o2);
          if ("throw" !== c2.type) {
            var u2 = c2.arg, h2 = u2.value;
            return h2 && "object" == _typeof2(h2) && n.call(h2, "__await") ? e2.resolve(h2.__await).then(function(t3) {
              invoke("next", t3, i2, a2);
            }, function(t3) {
              invoke("throw", t3, i2, a2);
            }) : e2.resolve(h2).then(function(t3) {
              u2.value = t3, i2(u2);
            }, function(t3) {
              return invoke("throw", t3, i2, a2);
            });
          }
          a2(c2.arg);
        }
        var r2;
        o(this, "_invoke", {
          value: function value(t3, n2) {
            function callInvokeWithMethodAndArg() {
              return new e2(function(e3, r3) {
                invoke(t3, n2, e3, r3);
              });
            }
            return r2 = r2 ? r2.then(callInvokeWithMethodAndArg, callInvokeWithMethodAndArg) : callInvokeWithMethodAndArg();
          }
        });
      }
      function makeInvokeMethod(e2, r2, n2) {
        var o2 = h;
        return function(i2, a2) {
          if (o2 === f) throw Error("Generator is already running");
          if (o2 === s) {
            if ("throw" === i2) throw a2;
            return {
              value: t,
              done: true
            };
          }
          for (n2.method = i2, n2.arg = a2; ; ) {
            var c2 = n2.delegate;
            if (c2) {
              var u2 = maybeInvokeDelegate(c2, n2);
              if (u2) {
                if (u2 === y) continue;
                return u2;
              }
            }
            if ("next" === n2.method) n2.sent = n2._sent = n2.arg;
            else if ("throw" === n2.method) {
              if (o2 === h) throw o2 = s, n2.arg;
              n2.dispatchException(n2.arg);
            } else "return" === n2.method && n2.abrupt("return", n2.arg);
            o2 = f;
            var p2 = tryCatch(e2, r2, n2);
            if ("normal" === p2.type) {
              if (o2 = n2.done ? s : l, p2.arg === y) continue;
              return {
                value: p2.arg,
                done: n2.done
              };
            }
            "throw" === p2.type && (o2 = s, n2.method = "throw", n2.arg = p2.arg);
          }
        };
      }
      function maybeInvokeDelegate(e2, r2) {
        var n2 = r2.method, o2 = e2.iterator[n2];
        if (o2 === t) return r2.delegate = null, "throw" === n2 && e2.iterator["return"] && (r2.method = "return", r2.arg = t, maybeInvokeDelegate(e2, r2), "throw" === r2.method) || "return" !== n2 && (r2.method = "throw", r2.arg = new TypeError("The iterator does not provide a '" + n2 + "' method")), y;
        var i2 = tryCatch(o2, e2.iterator, r2.arg);
        if ("throw" === i2.type) return r2.method = "throw", r2.arg = i2.arg, r2.delegate = null, y;
        var a2 = i2.arg;
        return a2 ? a2.done ? (r2[e2.resultName] = a2.value, r2.next = e2.nextLoc, "return" !== r2.method && (r2.method = "next", r2.arg = t), r2.delegate = null, y) : a2 : (r2.method = "throw", r2.arg = new TypeError("iterator result is not an object"), r2.delegate = null, y);
      }
      function pushTryEntry(t2) {
        var e2 = {
          tryLoc: t2[0]
        };
        1 in t2 && (e2.catchLoc = t2[1]), 2 in t2 && (e2.finallyLoc = t2[2], e2.afterLoc = t2[3]), this.tryEntries.push(e2);
      }
      function resetTryEntry(t2) {
        var e2 = t2.completion || {};
        e2.type = "normal", delete e2.arg, t2.completion = e2;
      }
      function Context(t2) {
        this.tryEntries = [{
          tryLoc: "root"
        }], t2.forEach(pushTryEntry, this), this.reset(true);
      }
      function values(e2) {
        if (e2 || "" === e2) {
          var r2 = e2[a];
          if (r2) return r2.call(e2);
          if ("function" == typeof e2.next) return e2;
          if (!isNaN(e2.length)) {
            var o2 = -1, i2 = function next() {
              for (; ++o2 < e2.length; ) if (n.call(e2, o2)) return next.value = e2[o2], next.done = false, next;
              return next.value = t, next.done = true, next;
            };
            return i2.next = i2;
          }
        }
        throw new TypeError(_typeof2(e2) + " is not iterable");
      }
      return GeneratorFunction.prototype = GeneratorFunctionPrototype, o(g, "constructor", {
        value: GeneratorFunctionPrototype,
        configurable: true
      }), o(GeneratorFunctionPrototype, "constructor", {
        value: GeneratorFunction,
        configurable: true
      }), GeneratorFunction.displayName = define(GeneratorFunctionPrototype, u, "GeneratorFunction"), e.isGeneratorFunction = function(t2) {
        var e2 = "function" == typeof t2 && t2.constructor;
        return !!e2 && (e2 === GeneratorFunction || "GeneratorFunction" === (e2.displayName || e2.name));
      }, e.mark = function(t2) {
        return Object.setPrototypeOf ? Object.setPrototypeOf(t2, GeneratorFunctionPrototype) : (t2.__proto__ = GeneratorFunctionPrototype, define(t2, u, "GeneratorFunction")), t2.prototype = Object.create(g), t2;
      }, e.awrap = function(t2) {
        return {
          __await: t2
        };
      }, defineIteratorMethods(AsyncIterator.prototype), define(AsyncIterator.prototype, c, function() {
        return this;
      }), e.AsyncIterator = AsyncIterator, e.async = function(t2, r2, n2, o2, i2) {
        void 0 === i2 && (i2 = Promise);
        var a2 = new AsyncIterator(wrap(t2, r2, n2, o2), i2);
        return e.isGeneratorFunction(r2) ? a2 : a2.next().then(function(t3) {
          return t3.done ? t3.value : a2.next();
        });
      }, defineIteratorMethods(g), define(g, u, "Generator"), define(g, a, function() {
        return this;
      }), define(g, "toString", function() {
        return "[object Generator]";
      }), e.keys = function(t2) {
        var e2 = Object(t2), r2 = [];
        for (var n2 in e2) r2.push(n2);
        return r2.reverse(), function next() {
          for (; r2.length; ) {
            var t3 = r2.pop();
            if (t3 in e2) return next.value = t3, next.done = false, next;
          }
          return next.done = true, next;
        };
      }, e.values = values, Context.prototype = {
        constructor: Context,
        reset: function reset(e2) {
          if (this.prev = 0, this.next = 0, this.sent = this._sent = t, this.done = false, this.delegate = null, this.method = "next", this.arg = t, this.tryEntries.forEach(resetTryEntry), !e2) for (var r2 in this) "t" === r2.charAt(0) && n.call(this, r2) && !isNaN(+r2.slice(1)) && (this[r2] = t);
        },
        stop: function stop() {
          this.done = true;
          var t2 = this.tryEntries[0].completion;
          if ("throw" === t2.type) throw t2.arg;
          return this.rval;
        },
        dispatchException: function dispatchException(e2) {
          if (this.done) throw e2;
          var r2 = this;
          function handle(n2, o3) {
            return a2.type = "throw", a2.arg = e2, r2.next = n2, o3 && (r2.method = "next", r2.arg = t), !!o3;
          }
          for (var o2 = this.tryEntries.length - 1; o2 >= 0; --o2) {
            var i2 = this.tryEntries[o2], a2 = i2.completion;
            if ("root" === i2.tryLoc) return handle("end");
            if (i2.tryLoc <= this.prev) {
              var c2 = n.call(i2, "catchLoc"), u2 = n.call(i2, "finallyLoc");
              if (c2 && u2) {
                if (this.prev < i2.catchLoc) return handle(i2.catchLoc, true);
                if (this.prev < i2.finallyLoc) return handle(i2.finallyLoc);
              } else if (c2) {
                if (this.prev < i2.catchLoc) return handle(i2.catchLoc, true);
              } else {
                if (!u2) throw Error("try statement without catch or finally");
                if (this.prev < i2.finallyLoc) return handle(i2.finallyLoc);
              }
            }
          }
        },
        abrupt: function abrupt(t2, e2) {
          for (var r2 = this.tryEntries.length - 1; r2 >= 0; --r2) {
            var o2 = this.tryEntries[r2];
            if (o2.tryLoc <= this.prev && n.call(o2, "finallyLoc") && this.prev < o2.finallyLoc) {
              var i2 = o2;
              break;
            }
          }
          i2 && ("break" === t2 || "continue" === t2) && i2.tryLoc <= e2 && e2 <= i2.finallyLoc && (i2 = null);
          var a2 = i2 ? i2.completion : {};
          return a2.type = t2, a2.arg = e2, i2 ? (this.method = "next", this.next = i2.finallyLoc, y) : this.complete(a2);
        },
        complete: function complete(t2, e2) {
          if ("throw" === t2.type) throw t2.arg;
          return "break" === t2.type || "continue" === t2.type ? this.next = t2.arg : "return" === t2.type ? (this.rval = this.arg = t2.arg, this.method = "return", this.next = "end") : "normal" === t2.type && e2 && (this.next = e2), y;
        },
        finish: function finish(t2) {
          for (var e2 = this.tryEntries.length - 1; e2 >= 0; --e2) {
            var r2 = this.tryEntries[e2];
            if (r2.finallyLoc === t2) return this.complete(r2.completion, r2.afterLoc), resetTryEntry(r2), y;
          }
        },
        "catch": function _catch(t2) {
          for (var e2 = this.tryEntries.length - 1; e2 >= 0; --e2) {
            var r2 = this.tryEntries[e2];
            if (r2.tryLoc === t2) {
              var n2 = r2.completion;
              if ("throw" === n2.type) {
                var o2 = n2.arg;
                resetTryEntry(r2);
              }
              return o2;
            }
          }
          throw Error("illegal catch attempt");
        },
        delegateYield: function delegateYield(e2, r2, n2) {
          return this.delegate = {
            iterator: values(e2),
            resultName: r2,
            nextLoc: n2
          }, "next" === this.method && (this.arg = t), y;
        }
      }, e;
    }
    module.exports = _regeneratorRuntime, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/regenerator/index.js
var require_regenerator = __commonJS({
  "node_modules/@babel/runtime/regenerator/index.js"(exports, module) {
    var runtime = require_regeneratorRuntime()();
    module.exports = runtime;
    try {
      regeneratorRuntime = runtime;
    } catch (accidentalStrictMode) {
      if (typeof globalThis === "object") {
        globalThis.regeneratorRuntime = runtime;
      } else {
        Function("r", "regeneratorRuntime = r")(runtime);
      }
    }
  }
});

// node_modules/@babel/runtime/helpers/extends.js
var require_extends = __commonJS({
  "node_modules/@babel/runtime/helpers/extends.js"(exports, module) {
    function _extends2() {
      return module.exports = _extends2 = Object.assign ? Object.assign.bind() : function(n) {
        for (var e = 1; e < arguments.length; e++) {
          var t = arguments[e];
          for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]);
        }
        return n;
      }, module.exports.__esModule = true, module.exports["default"] = module.exports, _extends2.apply(null, arguments);
    }
    module.exports = _extends2, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/asyncToGenerator.js
var require_asyncToGenerator = __commonJS({
  "node_modules/@babel/runtime/helpers/asyncToGenerator.js"(exports, module) {
    function asyncGeneratorStep(n, t, e, r, o, a, c) {
      try {
        var i = n[a](c), u = i.value;
      } catch (n2) {
        return void e(n2);
      }
      i.done ? t(u) : Promise.resolve(u).then(r, o);
    }
    function _asyncToGenerator(n) {
      return function() {
        var t = this, e = arguments;
        return new Promise(function(r, o) {
          var a = n.apply(t, e);
          function _next(n2) {
            asyncGeneratorStep(a, r, o, _next, _throw, "next", n2);
          }
          function _throw(n2) {
            asyncGeneratorStep(a, r, o, _next, _throw, "throw", n2);
          }
          _next(void 0);
        });
      };
    }
    module.exports = _asyncToGenerator, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/objectWithoutPropertiesLoose.js
var require_objectWithoutPropertiesLoose = __commonJS({
  "node_modules/@babel/runtime/helpers/objectWithoutPropertiesLoose.js"(exports, module) {
    function _objectWithoutPropertiesLoose(r, e) {
      if (null == r) return {};
      var t = {};
      for (var n in r) if ({}.hasOwnProperty.call(r, n)) {
        if (-1 !== e.indexOf(n)) continue;
        t[n] = r[n];
      }
      return t;
    }
    module.exports = _objectWithoutPropertiesLoose, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/objectWithoutProperties.js
var require_objectWithoutProperties = __commonJS({
  "node_modules/@babel/runtime/helpers/objectWithoutProperties.js"(exports, module) {
    var objectWithoutPropertiesLoose = require_objectWithoutPropertiesLoose();
    function _objectWithoutProperties(e, t) {
      if (null == e) return {};
      var o, r, i = objectWithoutPropertiesLoose(e, t);
      if (Object.getOwnPropertySymbols) {
        var n = Object.getOwnPropertySymbols(e);
        for (r = 0; r < n.length; r++) o = n[r], -1 === t.indexOf(o) && {}.propertyIsEnumerable.call(e, o) && (i[o] = e[o]);
      }
      return i;
    }
    module.exports = _objectWithoutProperties, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/govuk-react-jsx/govuk/components/accordion/index.js
var require_accordion = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/accordion/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Accordion = Accordion;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _excluded = ["headingLevel", "items", "className", "id"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function Accordion(props) {
      var accordionRef = (0, _react.useRef)();
      var headingLevel = props.headingLevel, items = props.items, className = props.className, id = props.id, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, AccordionJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-accordion" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./accordion-6DYGRLTA.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  AccordionJS = _yield$import["default"];
                  if (accordionRef.current) {
                    new AccordionJS(accordionRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [accordionRef]);
      var HeadingLevel = headingLevel ? "h".concat(headingLevel) : "h2";
      var innerHtml = items.map(function(item, index) {
        if (!item) {
          return;
        }
        return _react["default"].createElement("div", {
          key: item.reactListKey || index,
          className: "govuk-accordion__section ".concat(item.expanded ? "govuk-accordion__section--expanded" : "")
        }, _react["default"].createElement("div", {
          className: "govuk-accordion__section-header"
        }, _react["default"].createElement(HeadingLevel, {
          className: "govuk-accordion__section-heading"
        }, _react["default"].createElement("span", {
          className: "govuk-accordion__section-button",
          id: "".concat(id, "-heading-").concat(index + 1)
        }, item.heading.children)), item.summary ? _react["default"].createElement("div", {
          className: "govuk-accordion__section-summary govuk-body",
          id: "".concat(id, "-summary-").concat(index + 1)
        }, item.summary.children) : ""), _react["default"].createElement("div", {
          id: "".concat(id, "-content-").concat(index + 1),
          className: "govuk-accordion__section-content",
          "aria-labelledby": "".concat(id, "-heading-").concat(index + 1)
        }, item.content.children));
      });
      return _react["default"].createElement("div", (0, _extends2["default"])({}, attributes, {
        id,
        className: "govuk-accordion ".concat(className || ""),
        "data-module": "govuk-accordion",
        ref: accordionRef
      }), innerHtml);
    }
  }
});

// node_modules/turbo-stream/dist/utils.js
var require_utils = __commonJS({
  "node_modules/turbo-stream/dist/utils.js"(exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.createLineSplittingTransform = exports.Deferred = exports.TYPE_PREVIOUS_RESOLVED = exports.TYPE_URL = exports.TYPE_SYMBOL = exports.TYPE_SET = exports.TYPE_REGEXP = exports.TYPE_PROMISE = exports.TYPE_NULL_OBJECT = exports.TYPE_MAP = exports.TYPE_ERROR = exports.TYPE_DATE = exports.TYPE_BIGINT = exports.UNDEFINED = exports.POSITIVE_INFINITY = exports.NULL = exports.NEGATIVE_ZERO = exports.NEGATIVE_INFINITY = exports.NAN = exports.HOLE = void 0;
    exports.HOLE = -1;
    exports.NAN = -2;
    exports.NEGATIVE_INFINITY = -3;
    exports.NEGATIVE_ZERO = -4;
    exports.NULL = -5;
    exports.POSITIVE_INFINITY = -6;
    exports.UNDEFINED = -7;
    exports.TYPE_BIGINT = "B";
    exports.TYPE_DATE = "D";
    exports.TYPE_ERROR = "E";
    exports.TYPE_MAP = "M";
    exports.TYPE_NULL_OBJECT = "N";
    exports.TYPE_PROMISE = "P";
    exports.TYPE_REGEXP = "R";
    exports.TYPE_SET = "S";
    exports.TYPE_SYMBOL = "Y";
    exports.TYPE_URL = "U";
    exports.TYPE_PREVIOUS_RESOLVED = "Z";
    var Deferred = class {
      constructor() {
        __publicField(this, "promise");
        __publicField(this, "resolve");
        __publicField(this, "reject");
        this.promise = new Promise((resolve, reject) => {
          this.resolve = resolve;
          this.reject = reject;
        });
      }
    };
    exports.Deferred = Deferred;
    function createLineSplittingTransform() {
      const decoder = new TextDecoder();
      let leftover = "";
      return new TransformStream({
        transform(chunk, controller) {
          const str = decoder.decode(chunk, { stream: true });
          const parts = (leftover + str).split("\n");
          leftover = parts.pop() || "";
          for (const part of parts) {
            controller.enqueue(part);
          }
        },
        flush(controller) {
          if (leftover) {
            controller.enqueue(leftover);
          }
        }
      });
    }
    exports.createLineSplittingTransform = createLineSplittingTransform;
  }
});

// node_modules/turbo-stream/dist/flatten.js
var require_flatten = __commonJS({
  "node_modules/turbo-stream/dist/flatten.js"(exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.flatten = void 0;
    var utils_js_1 = require_utils();
    function flatten(input) {
      const { indices } = this;
      const existing = indices.get(input);
      if (existing)
        return [existing];
      if (input === void 0)
        return utils_js_1.UNDEFINED;
      if (input === null)
        return utils_js_1.NULL;
      if (Number.isNaN(input))
        return utils_js_1.NAN;
      if (input === Number.POSITIVE_INFINITY)
        return utils_js_1.POSITIVE_INFINITY;
      if (input === Number.NEGATIVE_INFINITY)
        return utils_js_1.NEGATIVE_INFINITY;
      if (input === 0 && 1 / input < 0)
        return utils_js_1.NEGATIVE_ZERO;
      const index = this.index++;
      indices.set(input, index);
      stringify.call(this, input, index);
      return index;
    }
    exports.flatten = flatten;
    function stringify(input, index) {
      const { deferred, plugins, postPlugins } = this;
      const str = this.stringified;
      const stack = [[input, index]];
      while (stack.length > 0) {
        const [input2, index2] = stack.pop();
        const partsForObj = (obj) => Object.keys(obj).map((k) => `"_${flatten.call(this, k)}":${flatten.call(this, obj[k])}`).join(",");
        let error = null;
        switch (typeof input2) {
          case "boolean":
          case "number":
          case "string":
            str[index2] = JSON.stringify(input2);
            break;
          case "bigint":
            str[index2] = `["${utils_js_1.TYPE_BIGINT}","${input2}"]`;
            break;
          case "symbol": {
            const keyFor = Symbol.keyFor(input2);
            if (!keyFor) {
              error = new Error("Cannot encode symbol unless created with Symbol.for()");
            } else {
              str[index2] = `["${utils_js_1.TYPE_SYMBOL}",${JSON.stringify(keyFor)}]`;
            }
            break;
          }
          case "object": {
            if (!input2) {
              str[index2] = `${utils_js_1.NULL}`;
              break;
            }
            const isArray = Array.isArray(input2);
            let pluginHandled = false;
            if (!isArray && plugins) {
              for (const plugin of plugins) {
                const pluginResult = plugin(input2);
                if (Array.isArray(pluginResult)) {
                  pluginHandled = true;
                  const [pluginIdentifier, ...rest] = pluginResult;
                  str[index2] = `[${JSON.stringify(pluginIdentifier)}`;
                  if (rest.length > 0) {
                    str[index2] += `,${rest.map((v) => flatten.call(this, v)).join(",")}`;
                  }
                  str[index2] += "]";
                  break;
                }
              }
            }
            if (!pluginHandled) {
              let result = isArray ? "[" : "{";
              if (isArray) {
                for (let i = 0; i < input2.length; i++)
                  result += (i ? "," : "") + (i in input2 ? flatten.call(this, input2[i]) : utils_js_1.HOLE);
                str[index2] = `${result}]`;
              } else if (input2 instanceof Date) {
                str[index2] = `["${utils_js_1.TYPE_DATE}",${input2.getTime()}]`;
              } else if (input2 instanceof URL) {
                str[index2] = `["${utils_js_1.TYPE_URL}",${JSON.stringify(input2.href)}]`;
              } else if (input2 instanceof RegExp) {
                str[index2] = `["${utils_js_1.TYPE_REGEXP}",${JSON.stringify(input2.source)},${JSON.stringify(input2.flags)}]`;
              } else if (input2 instanceof Set) {
                if (input2.size > 0) {
                  str[index2] = `["${utils_js_1.TYPE_SET}",${[...input2].map((val) => flatten.call(this, val)).join(",")}]`;
                } else {
                  str[index2] = `["${utils_js_1.TYPE_SET}"]`;
                }
              } else if (input2 instanceof Map) {
                if (input2.size > 0) {
                  str[index2] = `["${utils_js_1.TYPE_MAP}",${[...input2].flatMap(([k, v]) => [
                    flatten.call(this, k),
                    flatten.call(this, v)
                  ]).join(",")}]`;
                } else {
                  str[index2] = `["${utils_js_1.TYPE_MAP}"]`;
                }
              } else if (input2 instanceof Promise) {
                str[index2] = `["${utils_js_1.TYPE_PROMISE}",${index2}]`;
                deferred[index2] = input2;
              } else if (input2 instanceof Error) {
                str[index2] = `["${utils_js_1.TYPE_ERROR}",${JSON.stringify(input2.message)}`;
                if (input2.name !== "Error") {
                  str[index2] += `,${JSON.stringify(input2.name)}`;
                }
                str[index2] += "]";
              } else if (Object.getPrototypeOf(input2) === null) {
                str[index2] = `["${utils_js_1.TYPE_NULL_OBJECT}",{${partsForObj(input2)}}]`;
              } else if (isPlainObject(input2)) {
                str[index2] = `{${partsForObj(input2)}}`;
              } else {
                error = new Error("Cannot encode object with prototype");
              }
            }
            break;
          }
          default: {
            const isArray = Array.isArray(input2);
            let pluginHandled = false;
            if (!isArray && plugins) {
              for (const plugin of plugins) {
                const pluginResult = plugin(input2);
                if (Array.isArray(pluginResult)) {
                  pluginHandled = true;
                  const [pluginIdentifier, ...rest] = pluginResult;
                  str[index2] = `[${JSON.stringify(pluginIdentifier)}`;
                  if (rest.length > 0) {
                    str[index2] += `,${rest.map((v) => flatten.call(this, v)).join(",")}`;
                  }
                  str[index2] += "]";
                  break;
                }
              }
            }
            if (!pluginHandled) {
              error = new Error("Cannot encode function or unexpected type");
            }
          }
        }
        if (error) {
          let pluginHandled = false;
          if (postPlugins) {
            for (const plugin of postPlugins) {
              const pluginResult = plugin(input2);
              if (Array.isArray(pluginResult)) {
                pluginHandled = true;
                const [pluginIdentifier, ...rest] = pluginResult;
                str[index2] = `[${JSON.stringify(pluginIdentifier)}`;
                if (rest.length > 0) {
                  str[index2] += `,${rest.map((v) => flatten.call(this, v)).join(",")}`;
                }
                str[index2] += "]";
                break;
              }
            }
          }
          if (!pluginHandled) {
            throw error;
          }
        }
      }
    }
    var objectProtoNames = Object.getOwnPropertyNames(Object.prototype).sort().join("\0");
    function isPlainObject(thing) {
      const proto = Object.getPrototypeOf(thing);
      return proto === Object.prototype || proto === null || Object.getOwnPropertyNames(proto).sort().join("\0") === objectProtoNames;
    }
  }
});

// node_modules/turbo-stream/dist/unflatten.js
var require_unflatten = __commonJS({
  "node_modules/turbo-stream/dist/unflatten.js"(exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.unflatten = void 0;
    var utils_js_1 = require_utils();
    var globalObj = typeof window !== "undefined" ? window : typeof globalThis !== "undefined" ? globalThis : void 0;
    function unflatten(parsed) {
      const { hydrated, values } = this;
      if (typeof parsed === "number")
        return hydrate.call(this, parsed);
      if (!Array.isArray(parsed) || !parsed.length)
        throw new SyntaxError();
      const startIndex = values.length;
      for (const value of parsed) {
        values.push(value);
      }
      hydrated.length = values.length;
      return hydrate.call(this, startIndex);
    }
    exports.unflatten = unflatten;
    function hydrate(index) {
      const { hydrated, values, deferred, plugins } = this;
      let result;
      const stack = [
        [
          index,
          (v) => {
            result = v;
          }
        ]
      ];
      let postRun = [];
      while (stack.length > 0) {
        const [index2, set] = stack.pop();
        switch (index2) {
          case utils_js_1.UNDEFINED:
            set(void 0);
            continue;
          case utils_js_1.NULL:
            set(null);
            continue;
          case utils_js_1.NAN:
            set(NaN);
            continue;
          case utils_js_1.POSITIVE_INFINITY:
            set(Infinity);
            continue;
          case utils_js_1.NEGATIVE_INFINITY:
            set(-Infinity);
            continue;
          case utils_js_1.NEGATIVE_ZERO:
            set(-0);
            continue;
        }
        if (hydrated[index2]) {
          set(hydrated[index2]);
          continue;
        }
        const value = values[index2];
        if (!value || typeof value !== "object") {
          hydrated[index2] = value;
          set(value);
          continue;
        }
        if (Array.isArray(value)) {
          if (typeof value[0] === "string") {
            const [type, b, c] = value;
            switch (type) {
              case utils_js_1.TYPE_DATE:
                set(hydrated[index2] = new Date(b));
                continue;
              case utils_js_1.TYPE_URL:
                set(hydrated[index2] = new URL(b));
                continue;
              case utils_js_1.TYPE_BIGINT:
                set(hydrated[index2] = BigInt(b));
                continue;
              case utils_js_1.TYPE_REGEXP:
                set(hydrated[index2] = new RegExp(b, c));
                continue;
              case utils_js_1.TYPE_SYMBOL:
                set(hydrated[index2] = Symbol.for(b));
                continue;
              case utils_js_1.TYPE_SET:
                const newSet = /* @__PURE__ */ new Set();
                hydrated[index2] = newSet;
                for (let i = 1; i < value.length; i++)
                  stack.push([
                    value[i],
                    (v) => {
                      newSet.add(v);
                    }
                  ]);
                set(newSet);
                continue;
              case utils_js_1.TYPE_MAP:
                const map = /* @__PURE__ */ new Map();
                hydrated[index2] = map;
                for (let i = 1; i < value.length; i += 2) {
                  const r = [];
                  stack.push([
                    value[i + 1],
                    (v) => {
                      r[1] = v;
                    }
                  ]);
                  stack.push([
                    value[i],
                    (k) => {
                      r[0] = k;
                    }
                  ]);
                  postRun.push(() => {
                    map.set(r[0], r[1]);
                  });
                }
                set(map);
                continue;
              case utils_js_1.TYPE_NULL_OBJECT:
                const obj = /* @__PURE__ */ Object.create(null);
                hydrated[index2] = obj;
                for (const key of Object.keys(b).reverse()) {
                  const r = [];
                  stack.push([
                    b[key],
                    (v) => {
                      r[1] = v;
                    }
                  ]);
                  stack.push([
                    Number(key.slice(1)),
                    (k) => {
                      r[0] = k;
                    }
                  ]);
                  postRun.push(() => {
                    obj[r[0]] = r[1];
                  });
                }
                set(obj);
                continue;
              case utils_js_1.TYPE_PROMISE:
                if (hydrated[b]) {
                  set(hydrated[index2] = hydrated[b]);
                } else {
                  const d = new utils_js_1.Deferred();
                  deferred[b] = d;
                  set(hydrated[index2] = d.promise);
                }
                continue;
              case utils_js_1.TYPE_ERROR:
                const [, message, errorType] = value;
                let error = errorType && globalObj && globalObj[errorType] ? new globalObj[errorType](message) : new Error(message);
                hydrated[index2] = error;
                set(error);
                continue;
              case utils_js_1.TYPE_PREVIOUS_RESOLVED:
                set(hydrated[index2] = hydrated[b]);
                continue;
              default:
                if (Array.isArray(plugins)) {
                  const r = [];
                  const vals = value.slice(1);
                  for (let i = 0; i < vals.length; i++) {
                    const v = vals[i];
                    stack.push([
                      v,
                      (v2) => {
                        r[i] = v2;
                      }
                    ]);
                  }
                  postRun.push(() => {
                    for (const plugin of plugins) {
                      const result2 = plugin(value[0], ...r);
                      if (result2) {
                        set(hydrated[index2] = result2.value);
                        return;
                      }
                    }
                    throw new SyntaxError();
                  });
                  continue;
                }
                throw new SyntaxError();
            }
          } else {
            const array = [];
            hydrated[index2] = array;
            for (let i = 0; i < value.length; i++) {
              const n = value[i];
              if (n !== utils_js_1.HOLE) {
                stack.push([
                  n,
                  (v) => {
                    array[i] = v;
                  }
                ]);
              }
            }
            set(array);
            continue;
          }
        } else {
          const object = {};
          hydrated[index2] = object;
          for (const key of Object.keys(value).reverse()) {
            const r = [];
            stack.push([
              value[key],
              (v) => {
                r[1] = v;
              }
            ]);
            stack.push([
              Number(key.slice(1)),
              (k) => {
                r[0] = k;
              }
            ]);
            postRun.push(() => {
              object[r[0]] = r[1];
            });
          }
          set(object);
          continue;
        }
      }
      while (postRun.length > 0) {
        postRun.pop()();
      }
      return result;
    }
  }
});

// node_modules/turbo-stream/dist/turbo-stream.js
var require_turbo_stream = __commonJS({
  "node_modules/turbo-stream/dist/turbo-stream.js"(exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.encode = exports.decode = void 0;
    var flatten_js_1 = require_flatten();
    var unflatten_js_1 = require_unflatten();
    var utils_js_1 = require_utils();
    async function decode(readable, options) {
      const { plugins } = options ?? {};
      const done = new utils_js_1.Deferred();
      const reader = readable.pipeThrough((0, utils_js_1.createLineSplittingTransform)()).getReader();
      const decoder = {
        values: [],
        hydrated: [],
        deferred: {},
        plugins
      };
      const decoded = await decodeInitial.call(decoder, reader);
      let donePromise = done.promise;
      if (decoded.done) {
        done.resolve();
      } else {
        donePromise = decodeDeferred.call(decoder, reader).then(done.resolve).catch((reason) => {
          for (const deferred of Object.values(decoder.deferred)) {
            deferred.reject(reason);
          }
          done.reject(reason);
        });
      }
      return {
        done: donePromise.then(() => reader.closed),
        value: decoded.value
      };
    }
    exports.decode = decode;
    async function decodeInitial(reader) {
      const read = await reader.read();
      if (!read.value) {
        throw new SyntaxError();
      }
      let line;
      try {
        line = JSON.parse(read.value);
      } catch (reason) {
        throw new SyntaxError();
      }
      return {
        done: read.done,
        value: unflatten_js_1.unflatten.call(this, line)
      };
    }
    async function decodeDeferred(reader) {
      let read = await reader.read();
      while (!read.done) {
        if (!read.value)
          continue;
        const line = read.value;
        switch (line[0]) {
          case utils_js_1.TYPE_PROMISE: {
            const colonIndex = line.indexOf(":");
            const deferredId = Number(line.slice(1, colonIndex));
            const deferred = this.deferred[deferredId];
            if (!deferred) {
              throw new Error(`Deferred ID ${deferredId} not found in stream`);
            }
            const lineData = line.slice(colonIndex + 1);
            let jsonLine;
            try {
              jsonLine = JSON.parse(lineData);
            } catch (reason) {
              throw new SyntaxError();
            }
            const value = unflatten_js_1.unflatten.call(this, jsonLine);
            deferred.resolve(value);
            break;
          }
          case utils_js_1.TYPE_ERROR: {
            const colonIndex = line.indexOf(":");
            const deferredId = Number(line.slice(1, colonIndex));
            const deferred = this.deferred[deferredId];
            if (!deferred) {
              throw new Error(`Deferred ID ${deferredId} not found in stream`);
            }
            const lineData = line.slice(colonIndex + 1);
            let jsonLine;
            try {
              jsonLine = JSON.parse(lineData);
            } catch (reason) {
              throw new SyntaxError();
            }
            const value = unflatten_js_1.unflatten.call(this, jsonLine);
            deferred.reject(value);
            break;
          }
          default:
            throw new SyntaxError();
        }
        read = await reader.read();
      }
    }
    function encode(input, options) {
      const { plugins, postPlugins, signal } = options ?? {};
      const encoder = {
        deferred: {},
        index: 0,
        indices: /* @__PURE__ */ new Map(),
        stringified: [],
        plugins,
        postPlugins,
        signal
      };
      const textEncoder = new TextEncoder();
      let lastSentIndex = 0;
      const readable = new ReadableStream({
        async start(controller) {
          const id = flatten_js_1.flatten.call(encoder, input);
          if (Array.isArray(id)) {
            throw new Error("This should never happen");
          }
          if (id < 0) {
            controller.enqueue(textEncoder.encode(`${id}
`));
          } else {
            controller.enqueue(textEncoder.encode(`[${encoder.stringified.join(",")}]
`));
            lastSentIndex = encoder.stringified.length - 1;
          }
          const seenPromises = /* @__PURE__ */ new WeakSet();
          while (Object.keys(encoder.deferred).length > 0) {
            for (const [deferredId, deferred] of Object.entries(encoder.deferred)) {
              if (seenPromises.has(deferred))
                continue;
              seenPromises.add(encoder.deferred[Number(deferredId)] = raceSignal(deferred, encoder.signal).then((resolved) => {
                const id2 = flatten_js_1.flatten.call(encoder, resolved);
                if (Array.isArray(id2)) {
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_PROMISE}${deferredId}:[["${utils_js_1.TYPE_PREVIOUS_RESOLVED}",${id2[0]}]]
`));
                  encoder.index++;
                  lastSentIndex++;
                } else if (id2 < 0) {
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_PROMISE}${deferredId}:${id2}
`));
                } else {
                  const values = encoder.stringified.slice(lastSentIndex + 1).join(",");
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_PROMISE}${deferredId}:[${values}]
`));
                  lastSentIndex = encoder.stringified.length - 1;
                }
              }, (reason) => {
                if (!reason || typeof reason !== "object" || !(reason instanceof Error)) {
                  reason = new Error("An unknown error occurred");
                }
                const id2 = flatten_js_1.flatten.call(encoder, reason);
                if (Array.isArray(id2)) {
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_ERROR}${deferredId}:[["${utils_js_1.TYPE_PREVIOUS_RESOLVED}",${id2[0]}]]
`));
                  encoder.index++;
                  lastSentIndex++;
                } else if (id2 < 0) {
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_ERROR}${deferredId}:${id2}
`));
                } else {
                  const values = encoder.stringified.slice(lastSentIndex + 1).join(",");
                  controller.enqueue(textEncoder.encode(`${utils_js_1.TYPE_ERROR}${deferredId}:[${values}]
`));
                  lastSentIndex = encoder.stringified.length - 1;
                }
              }).finally(() => {
                delete encoder.deferred[Number(deferredId)];
              }));
            }
            await Promise.race(Object.values(encoder.deferred));
          }
          await Promise.all(Object.values(encoder.deferred));
          controller.close();
        }
      });
      return readable;
    }
    exports.encode = encode;
    function raceSignal(promise, signal) {
      if (!signal)
        return promise;
      if (signal.aborted)
        return Promise.reject(signal.reason || new Error("Signal was aborted."));
      const abort = new Promise((resolve, reject) => {
        signal.addEventListener("abort", (event) => {
          reject(signal.reason || new Error("Signal was aborted."));
        });
        promise.then(resolve).catch(reject);
      });
      abort.catch(() => {
      });
      return Promise.race([abort, promise]);
    }
  }
});

// node_modules/react-router/dist/development/dom-export.js
var require_dom_export = __commonJS({
  "node_modules/react-router/dist/development/dom-export.js"(exports, module) {
    "use strict";
    var __create = Object.create;
    var __defProp = Object.defineProperty;
    var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
    var __getOwnPropNames = Object.getOwnPropertyNames;
    var __getProtoOf = Object.getPrototypeOf;
    var __hasOwnProp = Object.prototype.hasOwnProperty;
    var __export2 = (target, all) => {
      for (var name in all)
        __defProp(target, name, { get: all[name], enumerable: true });
    };
    var __copyProps = (to, from, except, desc) => {
      if (from && typeof from === "object" || typeof from === "function") {
        for (let key of __getOwnPropNames(from))
          if (!__hasOwnProp.call(to, key) && key !== except)
            __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
      }
      return to;
    };
    var __toESM2 = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
      // If the importer is in node compatibility mode or this is not an ESM
      // file that has been converted to a CommonJS file using a Babel-
      // compatible transform (i.e. "__esModule" has not been set), then set
      // "default" to the CommonJS "module.exports" for node compatibility.
      isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
      mod
    ));
    var __toCommonJS2 = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
    var dom_export_exports = {};
    __export2(dom_export_exports, {
      HydratedRouter: () => HydratedRouter2,
      RouterProvider: () => RouterProvider22
    });
    module.exports = __toCommonJS2(dom_export_exports);
    var React10 = __toESM2(require_react());
    var ReactDOM = __toESM2(require_react_dom());
    var PopStateEventType = "popstate";
    function createBrowserHistory(options = {}) {
      function createBrowserLocation(window2, globalHistory) {
        let { pathname, search, hash } = window2.location;
        return createLocation(
          "",
          { pathname, search, hash },
          // state defaults to `null` because `window.history.state` does
          globalHistory.state && globalHistory.state.usr || null,
          globalHistory.state && globalHistory.state.key || "default"
        );
      }
      function createBrowserHref(window2, to) {
        return typeof to === "string" ? to : createPath(to);
      }
      return getUrlBasedHistory(
        createBrowserLocation,
        createBrowserHref,
        null,
        options
      );
    }
    function invariant(value, message) {
      if (value === false || value === null || typeof value === "undefined") {
        throw new Error(message);
      }
    }
    function warning(cond, message) {
      if (!cond) {
        if (typeof console !== "undefined") console.warn(message);
        try {
          throw new Error(message);
        } catch (e) {
        }
      }
    }
    function createKey() {
      return Math.random().toString(36).substring(2, 10);
    }
    function getHistoryState(location, index) {
      return {
        usr: location.state,
        key: location.key,
        idx: index
      };
    }
    function createLocation(current, to, state = null, key) {
      let location = {
        pathname: typeof current === "string" ? current : current.pathname,
        search: "",
        hash: "",
        ...typeof to === "string" ? parsePath(to) : to,
        state,
        // TODO: This could be cleaned up.  push/replace should probably just take
        // full Locations now and avoid the need to run through this flow at all
        // But that's a pretty big refactor to the current test suite so going to
        // keep as is for the time being and just let any incoming keys take precedence
        key: to && to.key || key || createKey()
      };
      return location;
    }
    function createPath({
      pathname = "/",
      search = "",
      hash = ""
    }) {
      if (search && search !== "?")
        pathname += search.charAt(0) === "?" ? search : "?" + search;
      if (hash && hash !== "#")
        pathname += hash.charAt(0) === "#" ? hash : "#" + hash;
      return pathname;
    }
    function parsePath(path) {
      let parsedPath = {};
      if (path) {
        let hashIndex = path.indexOf("#");
        if (hashIndex >= 0) {
          parsedPath.hash = path.substring(hashIndex);
          path = path.substring(0, hashIndex);
        }
        let searchIndex = path.indexOf("?");
        if (searchIndex >= 0) {
          parsedPath.search = path.substring(searchIndex);
          path = path.substring(0, searchIndex);
        }
        if (path) {
          parsedPath.pathname = path;
        }
      }
      return parsedPath;
    }
    function getUrlBasedHistory(getLocation, createHref, validateLocation, options = {}) {
      let { window: window2 = document.defaultView, v5Compat = false } = options;
      let globalHistory = window2.history;
      let action = "POP";
      let listener = null;
      let index = getIndex();
      if (index == null) {
        index = 0;
        globalHistory.replaceState({ ...globalHistory.state, idx: index }, "");
      }
      function getIndex() {
        let state = globalHistory.state || { idx: null };
        return state.idx;
      }
      function handlePop() {
        action = "POP";
        let nextIndex = getIndex();
        let delta = nextIndex == null ? null : nextIndex - index;
        index = nextIndex;
        if (listener) {
          listener({ action, location: history.location, delta });
        }
      }
      function push(to, state) {
        action = "PUSH";
        let location = createLocation(history.location, to, state);
        if (validateLocation) validateLocation(location, to);
        index = getIndex() + 1;
        let historyState = getHistoryState(location, index);
        let url = history.createHref(location);
        try {
          globalHistory.pushState(historyState, "", url);
        } catch (error) {
          if (error instanceof DOMException && error.name === "DataCloneError") {
            throw error;
          }
          window2.location.assign(url);
        }
        if (v5Compat && listener) {
          listener({ action, location: history.location, delta: 1 });
        }
      }
      function replace2(to, state) {
        action = "REPLACE";
        let location = createLocation(history.location, to, state);
        if (validateLocation) validateLocation(location, to);
        index = getIndex();
        let historyState = getHistoryState(location, index);
        let url = history.createHref(location);
        globalHistory.replaceState(historyState, "", url);
        if (v5Compat && listener) {
          listener({ action, location: history.location, delta: 0 });
        }
      }
      function createURL(to) {
        let base = window2.location.origin !== "null" ? window2.location.origin : window2.location.href;
        let href = typeof to === "string" ? to : createPath(to);
        href = href.replace(/ $/, "%20");
        invariant(
          base,
          `No window.location.(origin|href) available to create URL for href: ${href}`
        );
        return new URL(href, base);
      }
      let history = {
        get action() {
          return action;
        },
        get location() {
          return getLocation(window2, globalHistory);
        },
        listen(fn) {
          if (listener) {
            throw new Error("A history only accepts one active listener");
          }
          window2.addEventListener(PopStateEventType, handlePop);
          listener = fn;
          return () => {
            window2.removeEventListener(PopStateEventType, handlePop);
            listener = null;
          };
        },
        createHref(to) {
          return createHref(window2, to);
        },
        createURL,
        encodeLocation(to) {
          let url = createURL(to);
          return {
            pathname: url.pathname,
            search: url.search,
            hash: url.hash
          };
        },
        push,
        replace: replace2,
        go(n) {
          return globalHistory.go(n);
        }
      };
      return history;
    }
    var immutableRouteKeys = /* @__PURE__ */ new Set([
      "lazy",
      "caseSensitive",
      "path",
      "id",
      "index",
      "children"
    ]);
    function isIndexRoute(route) {
      return route.index === true;
    }
    function convertRoutesToDataRoutes(routes, mapRouteProperties2, parentPath = [], manifest = {}) {
      return routes.map((route, index) => {
        let treePath = [...parentPath, String(index)];
        let id = typeof route.id === "string" ? route.id : treePath.join("-");
        invariant(
          route.index !== true || !route.children,
          `Cannot specify children on an index route`
        );
        invariant(
          !manifest[id],
          `Found a route id collision on id "${id}".  Route id's must be globally unique within Data Router usages`
        );
        if (isIndexRoute(route)) {
          let indexRoute = {
            ...route,
            ...mapRouteProperties2(route),
            id
          };
          manifest[id] = indexRoute;
          return indexRoute;
        } else {
          let pathOrLayoutRoute = {
            ...route,
            ...mapRouteProperties2(route),
            id,
            children: void 0
          };
          manifest[id] = pathOrLayoutRoute;
          if (route.children) {
            pathOrLayoutRoute.children = convertRoutesToDataRoutes(
              route.children,
              mapRouteProperties2,
              treePath,
              manifest
            );
          }
          return pathOrLayoutRoute;
        }
      });
    }
    function matchRoutes(routes, locationArg, basename = "/") {
      return matchRoutesImpl(routes, locationArg, basename, false);
    }
    function matchRoutesImpl(routes, locationArg, basename, allowPartial) {
      let location = typeof locationArg === "string" ? parsePath(locationArg) : locationArg;
      let pathname = stripBasename(location.pathname || "/", basename);
      if (pathname == null) {
        return null;
      }
      let branches = flattenRoutes(routes);
      rankRouteBranches(branches);
      let matches = null;
      for (let i = 0; matches == null && i < branches.length; ++i) {
        let decoded = decodePath(pathname);
        matches = matchRouteBranch(
          branches[i],
          decoded,
          allowPartial
        );
      }
      return matches;
    }
    function convertRouteMatchToUiMatch(match, loaderData) {
      let { route, pathname, params } = match;
      return {
        id: route.id,
        pathname,
        params,
        data: loaderData[route.id],
        handle: route.handle
      };
    }
    function flattenRoutes(routes, branches = [], parentsMeta = [], parentPath = "") {
      let flattenRoute = (route, index, relativePath) => {
        let meta = {
          relativePath: relativePath === void 0 ? route.path || "" : relativePath,
          caseSensitive: route.caseSensitive === true,
          childrenIndex: index,
          route
        };
        if (meta.relativePath.startsWith("/")) {
          invariant(
            meta.relativePath.startsWith(parentPath),
            `Absolute route path "${meta.relativePath}" nested under path "${parentPath}" is not valid. An absolute child route path must start with the combined path of all its parent routes.`
          );
          meta.relativePath = meta.relativePath.slice(parentPath.length);
        }
        let path = joinPaths([parentPath, meta.relativePath]);
        let routesMeta = parentsMeta.concat(meta);
        if (route.children && route.children.length > 0) {
          invariant(
            // Our types know better, but runtime JS may not!
            // @ts-expect-error
            route.index !== true,
            `Index routes must not have child routes. Please remove all child routes from route path "${path}".`
          );
          flattenRoutes(route.children, branches, routesMeta, path);
        }
        if (route.path == null && !route.index) {
          return;
        }
        branches.push({
          path,
          score: computeScore(path, route.index),
          routesMeta
        });
      };
      routes.forEach((route, index) => {
        var _a;
        if (route.path === "" || !((_a = route.path) == null ? void 0 : _a.includes("?"))) {
          flattenRoute(route, index);
        } else {
          for (let exploded of explodeOptionalSegments(route.path)) {
            flattenRoute(route, index, exploded);
          }
        }
      });
      return branches;
    }
    function explodeOptionalSegments(path) {
      let segments = path.split("/");
      if (segments.length === 0) return [];
      let [first, ...rest] = segments;
      let isOptional = first.endsWith("?");
      let required = first.replace(/\?$/, "");
      if (rest.length === 0) {
        return isOptional ? [required, ""] : [required];
      }
      let restExploded = explodeOptionalSegments(rest.join("/"));
      let result = [];
      result.push(
        ...restExploded.map(
          (subpath) => subpath === "" ? required : [required, subpath].join("/")
        )
      );
      if (isOptional) {
        result.push(...restExploded);
      }
      return result.map(
        (exploded) => path.startsWith("/") && exploded === "" ? "/" : exploded
      );
    }
    function rankRouteBranches(branches) {
      branches.sort(
        (a, b) => a.score !== b.score ? b.score - a.score : compareIndexes(
          a.routesMeta.map((meta) => meta.childrenIndex),
          b.routesMeta.map((meta) => meta.childrenIndex)
        )
      );
    }
    var paramRe = /^:[\w-]+$/;
    var dynamicSegmentValue = 3;
    var indexRouteValue = 2;
    var emptySegmentValue = 1;
    var staticSegmentValue = 10;
    var splatPenalty = -2;
    var isSplat = (s) => s === "*";
    function computeScore(path, index) {
      let segments = path.split("/");
      let initialScore = segments.length;
      if (segments.some(isSplat)) {
        initialScore += splatPenalty;
      }
      if (index) {
        initialScore += indexRouteValue;
      }
      return segments.filter((s) => !isSplat(s)).reduce(
        (score, segment) => score + (paramRe.test(segment) ? dynamicSegmentValue : segment === "" ? emptySegmentValue : staticSegmentValue),
        initialScore
      );
    }
    function compareIndexes(a, b) {
      let siblings = a.length === b.length && a.slice(0, -1).every((n, i) => n === b[i]);
      return siblings ? (
        // If two routes are siblings, we should try to match the earlier sibling
        // first. This allows people to have fine-grained control over the matching
        // behavior by simply putting routes with identical paths in the order they
        // want them tried.
        a[a.length - 1] - b[b.length - 1]
      ) : (
        // Otherwise, it doesn't really make sense to rank non-siblings by index,
        // so they sort equally.
        0
      );
    }
    function matchRouteBranch(branch, pathname, allowPartial = false) {
      let { routesMeta } = branch;
      let matchedParams = {};
      let matchedPathname = "/";
      let matches = [];
      for (let i = 0; i < routesMeta.length; ++i) {
        let meta = routesMeta[i];
        let end = i === routesMeta.length - 1;
        let remainingPathname = matchedPathname === "/" ? pathname : pathname.slice(matchedPathname.length) || "/";
        let match = matchPath(
          { path: meta.relativePath, caseSensitive: meta.caseSensitive, end },
          remainingPathname
        );
        let route = meta.route;
        if (!match && end && allowPartial && !routesMeta[routesMeta.length - 1].route.index) {
          match = matchPath(
            {
              path: meta.relativePath,
              caseSensitive: meta.caseSensitive,
              end: false
            },
            remainingPathname
          );
        }
        if (!match) {
          return null;
        }
        Object.assign(matchedParams, match.params);
        matches.push({
          // TODO: Can this as be avoided?
          params: matchedParams,
          pathname: joinPaths([matchedPathname, match.pathname]),
          pathnameBase: normalizePathname(
            joinPaths([matchedPathname, match.pathnameBase])
          ),
          route
        });
        if (match.pathnameBase !== "/") {
          matchedPathname = joinPaths([matchedPathname, match.pathnameBase]);
        }
      }
      return matches;
    }
    function matchPath(pattern, pathname) {
      if (typeof pattern === "string") {
        pattern = { path: pattern, caseSensitive: false, end: true };
      }
      let [matcher, compiledParams] = compilePath(
        pattern.path,
        pattern.caseSensitive,
        pattern.end
      );
      let match = pathname.match(matcher);
      if (!match) return null;
      let matchedPathname = match[0];
      let pathnameBase = matchedPathname.replace(/(.)\/+$/, "$1");
      let captureGroups = match.slice(1);
      let params = compiledParams.reduce(
        (memo2, { paramName, isOptional }, index) => {
          if (paramName === "*") {
            let splatValue = captureGroups[index] || "";
            pathnameBase = matchedPathname.slice(0, matchedPathname.length - splatValue.length).replace(/(.)\/+$/, "$1");
          }
          const value = captureGroups[index];
          if (isOptional && !value) {
            memo2[paramName] = void 0;
          } else {
            memo2[paramName] = (value || "").replace(/%2F/g, "/");
          }
          return memo2;
        },
        {}
      );
      return {
        params,
        pathname: matchedPathname,
        pathnameBase,
        pattern
      };
    }
    function compilePath(path, caseSensitive = false, end = true) {
      warning(
        path === "*" || !path.endsWith("*") || path.endsWith("/*"),
        `Route path "${path}" will be treated as if it were "${path.replace(/\*$/, "/*")}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${path.replace(/\*$/, "/*")}".`
      );
      let params = [];
      let regexpSource = "^" + path.replace(/\/*\*?$/, "").replace(/^\/*/, "/").replace(/[\\.*+^${}|()[\]]/g, "\\$&").replace(
        /\/:([\w-]+)(\?)?/g,
        (_, paramName, isOptional) => {
          params.push({ paramName, isOptional: isOptional != null });
          return isOptional ? "/?([^\\/]+)?" : "/([^\\/]+)";
        }
      );
      if (path.endsWith("*")) {
        params.push({ paramName: "*" });
        regexpSource += path === "*" || path === "/*" ? "(.*)$" : "(?:\\/(.+)|\\/*)$";
      } else if (end) {
        regexpSource += "\\/*$";
      } else if (path !== "" && path !== "/") {
        regexpSource += "(?:(?=\\/|$))";
      } else {
      }
      let matcher = new RegExp(regexpSource, caseSensitive ? void 0 : "i");
      return [matcher, params];
    }
    function decodePath(value) {
      try {
        return value.split("/").map((v) => decodeURIComponent(v).replace(/\//g, "%2F")).join("/");
      } catch (error) {
        warning(
          false,
          `The URL path "${value}" could not be decoded because it is a malformed URL segment. This is probably due to a bad percent encoding (${error}).`
        );
        return value;
      }
    }
    function stripBasename(pathname, basename) {
      if (basename === "/") return pathname;
      if (!pathname.toLowerCase().startsWith(basename.toLowerCase())) {
        return null;
      }
      let startIndex = basename.endsWith("/") ? basename.length - 1 : basename.length;
      let nextChar = pathname.charAt(startIndex);
      if (nextChar && nextChar !== "/") {
        return null;
      }
      return pathname.slice(startIndex) || "/";
    }
    function resolvePath(to, fromPathname = "/") {
      let {
        pathname: toPathname,
        search = "",
        hash = ""
      } = typeof to === "string" ? parsePath(to) : to;
      let pathname = toPathname ? toPathname.startsWith("/") ? toPathname : resolvePathname(toPathname, fromPathname) : fromPathname;
      return {
        pathname,
        search: normalizeSearch(search),
        hash: normalizeHash(hash)
      };
    }
    function resolvePathname(relativePath, fromPathname) {
      let segments = fromPathname.replace(/\/+$/, "").split("/");
      let relativeSegments = relativePath.split("/");
      relativeSegments.forEach((segment) => {
        if (segment === "..") {
          if (segments.length > 1) segments.pop();
        } else if (segment !== ".") {
          segments.push(segment);
        }
      });
      return segments.length > 1 ? segments.join("/") : "/";
    }
    function getInvalidPathError(char, field, dest, path) {
      return `Cannot include a '${char}' character in a manually specified \`to.${field}\` field [${JSON.stringify(
        path
      )}].  Please separate it out to the \`to.${dest}\` field. Alternatively you may provide the full path as a string in <Link to="..."> and the router will parse it for you.`;
    }
    function getPathContributingMatches(matches) {
      return matches.filter(
        (match, index) => index === 0 || match.route.path && match.route.path.length > 0
      );
    }
    function getResolveToMatches(matches) {
      let pathMatches = getPathContributingMatches(matches);
      return pathMatches.map(
        (match, idx) => idx === pathMatches.length - 1 ? match.pathname : match.pathnameBase
      );
    }
    function resolveTo(toArg, routePathnames, locationPathname, isPathRelative = false) {
      let to;
      if (typeof toArg === "string") {
        to = parsePath(toArg);
      } else {
        to = { ...toArg };
        invariant(
          !to.pathname || !to.pathname.includes("?"),
          getInvalidPathError("?", "pathname", "search", to)
        );
        invariant(
          !to.pathname || !to.pathname.includes("#"),
          getInvalidPathError("#", "pathname", "hash", to)
        );
        invariant(
          !to.search || !to.search.includes("#"),
          getInvalidPathError("#", "search", "hash", to)
        );
      }
      let isEmptyPath = toArg === "" || to.pathname === "";
      let toPathname = isEmptyPath ? "/" : to.pathname;
      let from;
      if (toPathname == null) {
        from = locationPathname;
      } else {
        let routePathnameIndex = routePathnames.length - 1;
        if (!isPathRelative && toPathname.startsWith("..")) {
          let toSegments = toPathname.split("/");
          while (toSegments[0] === "..") {
            toSegments.shift();
            routePathnameIndex -= 1;
          }
          to.pathname = toSegments.join("/");
        }
        from = routePathnameIndex >= 0 ? routePathnames[routePathnameIndex] : "/";
      }
      let path = resolvePath(to, from);
      let hasExplicitTrailingSlash = toPathname && toPathname !== "/" && toPathname.endsWith("/");
      let hasCurrentTrailingSlash = (isEmptyPath || toPathname === ".") && locationPathname.endsWith("/");
      if (!path.pathname.endsWith("/") && (hasExplicitTrailingSlash || hasCurrentTrailingSlash)) {
        path.pathname += "/";
      }
      return path;
    }
    var joinPaths = (paths) => paths.join("/").replace(/\/\/+/g, "/");
    var normalizePathname = (pathname) => pathname.replace(/\/+$/, "").replace(/^\/*/, "/");
    var normalizeSearch = (search) => !search || search === "?" ? "" : search.startsWith("?") ? search : "?" + search;
    var normalizeHash = (hash) => !hash || hash === "#" ? "" : hash.startsWith("#") ? hash : "#" + hash;
    var DataWithResponseInit = class {
      constructor(data2, init) {
        this.type = "DataWithResponseInit";
        this.data = data2;
        this.init = init || null;
      }
    };
    function data(data2, init) {
      return new DataWithResponseInit(
        data2,
        typeof init === "number" ? { status: init } : init
      );
    }
    var redirect = (url, init = 302) => {
      let responseInit = init;
      if (typeof responseInit === "number") {
        responseInit = { status: responseInit };
      } else if (typeof responseInit.status === "undefined") {
        responseInit.status = 302;
      }
      let headers = new Headers(responseInit.headers);
      headers.set("Location", url);
      return new Response(null, { ...responseInit, headers });
    };
    var ErrorResponseImpl = class {
      constructor(status, statusText, data2, internal = false) {
        this.status = status;
        this.statusText = statusText || "";
        this.internal = internal;
        if (data2 instanceof Error) {
          this.data = data2.toString();
          this.error = data2;
        } else {
          this.data = data2;
        }
      }
    };
    function isRouteErrorResponse(error) {
      return error != null && typeof error.status === "number" && typeof error.statusText === "string" && typeof error.internal === "boolean" && "data" in error;
    }
    var validMutationMethodsArr = [
      "POST",
      "PUT",
      "PATCH",
      "DELETE"
    ];
    var validMutationMethods = new Set(
      validMutationMethodsArr
    );
    var validRequestMethodsArr = [
      "GET",
      ...validMutationMethodsArr
    ];
    var validRequestMethods = new Set(validRequestMethodsArr);
    var redirectStatusCodes = /* @__PURE__ */ new Set([301, 302, 303, 307, 308]);
    var redirectPreserveMethodStatusCodes = /* @__PURE__ */ new Set([307, 308]);
    var IDLE_NAVIGATION = {
      state: "idle",
      location: void 0,
      formMethod: void 0,
      formAction: void 0,
      formEncType: void 0,
      formData: void 0,
      json: void 0,
      text: void 0
    };
    var IDLE_FETCHER = {
      state: "idle",
      data: void 0,
      formMethod: void 0,
      formAction: void 0,
      formEncType: void 0,
      formData: void 0,
      json: void 0,
      text: void 0
    };
    var IDLE_BLOCKER = {
      state: "unblocked",
      proceed: void 0,
      reset: void 0,
      location: void 0
    };
    var ABSOLUTE_URL_REGEX = /^(?:[a-z][a-z0-9+.-]*:|\/\/)/i;
    var defaultMapRouteProperties = (route) => ({
      hasErrorBoundary: Boolean(route.hasErrorBoundary)
    });
    var TRANSITIONS_STORAGE_KEY = "remix-router-transitions";
    var ResetLoaderDataSymbol = Symbol("ResetLoaderData");
    function createRouter(init) {
      const routerWindow = init.window ? init.window : typeof window !== "undefined" ? window : void 0;
      const isBrowser = typeof routerWindow !== "undefined" && typeof routerWindow.document !== "undefined" && typeof routerWindow.document.createElement !== "undefined";
      invariant(
        init.routes.length > 0,
        "You must provide a non-empty routes array to createRouter"
      );
      let mapRouteProperties2 = init.mapRouteProperties || defaultMapRouteProperties;
      let manifest = {};
      let dataRoutes = convertRoutesToDataRoutes(
        init.routes,
        mapRouteProperties2,
        void 0,
        manifest
      );
      let inFlightDataRoutes;
      let basename = init.basename || "/";
      let dataStrategyImpl = init.dataStrategy || defaultDataStrategy;
      let patchRoutesOnNavigationImpl = init.patchRoutesOnNavigation;
      let future = {
        ...init.future
      };
      let unlistenHistory = null;
      let subscribers = /* @__PURE__ */ new Set();
      let savedScrollPositions = null;
      let getScrollRestorationKey = null;
      let getScrollPosition = null;
      let initialScrollRestored = init.hydrationData != null;
      let initialMatches = matchRoutes(dataRoutes, init.history.location, basename);
      let initialMatchesIsFOW = false;
      let initialErrors = null;
      if (initialMatches == null && !patchRoutesOnNavigationImpl) {
        let error = getInternalRouterError(404, {
          pathname: init.history.location.pathname
        });
        let { matches, route } = getShortCircuitMatches(dataRoutes);
        initialMatches = matches;
        initialErrors = { [route.id]: error };
      }
      if (initialMatches && !init.hydrationData) {
        let fogOfWar = checkFogOfWar(
          initialMatches,
          dataRoutes,
          init.history.location.pathname
        );
        if (fogOfWar.active) {
          initialMatches = null;
        }
      }
      let initialized;
      if (!initialMatches) {
        initialized = false;
        initialMatches = [];
        let fogOfWar = checkFogOfWar(
          null,
          dataRoutes,
          init.history.location.pathname
        );
        if (fogOfWar.active && fogOfWar.matches) {
          initialMatchesIsFOW = true;
          initialMatches = fogOfWar.matches;
        }
      } else if (initialMatches.some((m) => m.route.lazy)) {
        initialized = false;
      } else if (!initialMatches.some((m) => m.route.loader)) {
        initialized = true;
      } else {
        let loaderData = init.hydrationData ? init.hydrationData.loaderData : null;
        let errors = init.hydrationData ? init.hydrationData.errors : null;
        if (errors) {
          let idx = initialMatches.findIndex(
            (m) => errors[m.route.id] !== void 0
          );
          initialized = initialMatches.slice(0, idx + 1).every((m) => !shouldLoadRouteOnHydration(m.route, loaderData, errors));
        } else {
          initialized = initialMatches.every(
            (m) => !shouldLoadRouteOnHydration(m.route, loaderData, errors)
          );
        }
      }
      let router2;
      let state = {
        historyAction: init.history.action,
        location: init.history.location,
        matches: initialMatches,
        initialized,
        navigation: IDLE_NAVIGATION,
        // Don't restore on initial updateState() if we were SSR'd
        restoreScrollPosition: init.hydrationData != null ? false : null,
        preventScrollReset: false,
        revalidation: "idle",
        loaderData: init.hydrationData && init.hydrationData.loaderData || {},
        actionData: init.hydrationData && init.hydrationData.actionData || null,
        errors: init.hydrationData && init.hydrationData.errors || initialErrors,
        fetchers: /* @__PURE__ */ new Map(),
        blockers: /* @__PURE__ */ new Map()
      };
      let pendingAction = "POP";
      let pendingPreventScrollReset = false;
      let pendingNavigationController;
      let pendingViewTransitionEnabled = false;
      let appliedViewTransitions = /* @__PURE__ */ new Map();
      let removePageHideEventListener = null;
      let isUninterruptedRevalidation = false;
      let isRevalidationRequired = false;
      let cancelledFetcherLoads = /* @__PURE__ */ new Set();
      let fetchControllers = /* @__PURE__ */ new Map();
      let incrementingLoadId = 0;
      let pendingNavigationLoadId = -1;
      let fetchReloadIds = /* @__PURE__ */ new Map();
      let fetchRedirectIds = /* @__PURE__ */ new Set();
      let fetchLoadMatches = /* @__PURE__ */ new Map();
      let activeFetchers = /* @__PURE__ */ new Map();
      let fetchersQueuedForDeletion = /* @__PURE__ */ new Set();
      let blockerFunctions = /* @__PURE__ */ new Map();
      let unblockBlockerHistoryUpdate = void 0;
      let pendingRevalidationDfd = null;
      function initialize() {
        unlistenHistory = init.history.listen(
          ({ action: historyAction, location, delta }) => {
            if (unblockBlockerHistoryUpdate) {
              unblockBlockerHistoryUpdate();
              unblockBlockerHistoryUpdate = void 0;
              return;
            }
            warning(
              blockerFunctions.size === 0 || delta != null,
              "You are trying to use a blocker on a POP navigation to a location that was not created by @remix-run/router. This will fail silently in production. This can happen if you are navigating outside the router via `window.history.pushState`/`window.location.hash` instead of using router navigation APIs.  This can also happen if you are using createHashRouter and the user manually changes the URL."
            );
            let blockerKey = shouldBlockNavigation({
              currentLocation: state.location,
              nextLocation: location,
              historyAction
            });
            if (blockerKey && delta != null) {
              let nextHistoryUpdatePromise = new Promise((resolve) => {
                unblockBlockerHistoryUpdate = resolve;
              });
              init.history.go(delta * -1);
              updateBlocker(blockerKey, {
                state: "blocked",
                location,
                proceed() {
                  updateBlocker(blockerKey, {
                    state: "proceeding",
                    proceed: void 0,
                    reset: void 0,
                    location
                  });
                  nextHistoryUpdatePromise.then(() => init.history.go(delta));
                },
                reset() {
                  let blockers = new Map(state.blockers);
                  blockers.set(blockerKey, IDLE_BLOCKER);
                  updateState({ blockers });
                }
              });
              return;
            }
            return startNavigation(historyAction, location);
          }
        );
        if (isBrowser) {
          restoreAppliedTransitions(routerWindow, appliedViewTransitions);
          let _saveAppliedTransitions = () => persistAppliedTransitions(routerWindow, appliedViewTransitions);
          routerWindow.addEventListener("pagehide", _saveAppliedTransitions);
          removePageHideEventListener = () => routerWindow.removeEventListener("pagehide", _saveAppliedTransitions);
        }
        if (!state.initialized) {
          startNavigation("POP", state.location, {
            initialHydration: true
          });
        }
        return router2;
      }
      function dispose() {
        if (unlistenHistory) {
          unlistenHistory();
        }
        if (removePageHideEventListener) {
          removePageHideEventListener();
        }
        subscribers.clear();
        pendingNavigationController && pendingNavigationController.abort();
        state.fetchers.forEach((_, key) => deleteFetcher(key));
        state.blockers.forEach((_, key) => deleteBlocker(key));
      }
      function subscribe(fn) {
        subscribers.add(fn);
        return () => subscribers.delete(fn);
      }
      function updateState(newState, opts = {}) {
        state = {
          ...state,
          ...newState
        };
        let unmountedFetchers = [];
        let mountedFetchers = [];
        state.fetchers.forEach((fetcher, key) => {
          if (fetcher.state === "idle") {
            if (fetchersQueuedForDeletion.has(key)) {
              unmountedFetchers.push(key);
            } else {
              mountedFetchers.push(key);
            }
          }
        });
        fetchersQueuedForDeletion.forEach((key) => {
          if (!state.fetchers.has(key) && !fetchControllers.has(key)) {
            unmountedFetchers.push(key);
          }
        });
        [...subscribers].forEach(
          (subscriber) => subscriber(state, {
            deletedFetchers: unmountedFetchers,
            viewTransitionOpts: opts.viewTransitionOpts,
            flushSync: opts.flushSync === true
          })
        );
        unmountedFetchers.forEach((key) => deleteFetcher(key));
        mountedFetchers.forEach((key) => state.fetchers.delete(key));
      }
      function completeNavigation(location, newState, { flushSync: flushSync2 } = {}) {
        var _a, _b;
        let isActionReload = state.actionData != null && state.navigation.formMethod != null && isMutationMethod(state.navigation.formMethod) && state.navigation.state === "loading" && ((_a = location.state) == null ? void 0 : _a._isRedirect) !== true;
        let actionData;
        if (newState.actionData) {
          if (Object.keys(newState.actionData).length > 0) {
            actionData = newState.actionData;
          } else {
            actionData = null;
          }
        } else if (isActionReload) {
          actionData = state.actionData;
        } else {
          actionData = null;
        }
        let loaderData = newState.loaderData ? mergeLoaderData(
          state.loaderData,
          newState.loaderData,
          newState.matches || [],
          newState.errors
        ) : state.loaderData;
        let blockers = state.blockers;
        if (blockers.size > 0) {
          blockers = new Map(blockers);
          blockers.forEach((_, k) => blockers.set(k, IDLE_BLOCKER));
        }
        let preventScrollReset = pendingPreventScrollReset === true || state.navigation.formMethod != null && isMutationMethod(state.navigation.formMethod) && ((_b = location.state) == null ? void 0 : _b._isRedirect) !== true;
        if (inFlightDataRoutes) {
          dataRoutes = inFlightDataRoutes;
          inFlightDataRoutes = void 0;
        }
        if (isUninterruptedRevalidation) {
        } else if (pendingAction === "POP") {
        } else if (pendingAction === "PUSH") {
          init.history.push(location, location.state);
        } else if (pendingAction === "REPLACE") {
          init.history.replace(location, location.state);
        }
        let viewTransitionOpts;
        if (pendingAction === "POP") {
          let priorPaths = appliedViewTransitions.get(state.location.pathname);
          if (priorPaths && priorPaths.has(location.pathname)) {
            viewTransitionOpts = {
              currentLocation: state.location,
              nextLocation: location
            };
          } else if (appliedViewTransitions.has(location.pathname)) {
            viewTransitionOpts = {
              currentLocation: location,
              nextLocation: state.location
            };
          }
        } else if (pendingViewTransitionEnabled) {
          let toPaths = appliedViewTransitions.get(state.location.pathname);
          if (toPaths) {
            toPaths.add(location.pathname);
          } else {
            toPaths = /* @__PURE__ */ new Set([location.pathname]);
            appliedViewTransitions.set(state.location.pathname, toPaths);
          }
          viewTransitionOpts = {
            currentLocation: state.location,
            nextLocation: location
          };
        }
        updateState(
          {
            ...newState,
            // matches, errors, fetchers go through as-is
            actionData,
            loaderData,
            historyAction: pendingAction,
            location,
            initialized: true,
            navigation: IDLE_NAVIGATION,
            revalidation: "idle",
            restoreScrollPosition: getSavedScrollPosition(
              location,
              newState.matches || state.matches
            ),
            preventScrollReset,
            blockers
          },
          {
            viewTransitionOpts,
            flushSync: flushSync2 === true
          }
        );
        pendingAction = "POP";
        pendingPreventScrollReset = false;
        pendingViewTransitionEnabled = false;
        isUninterruptedRevalidation = false;
        isRevalidationRequired = false;
        pendingRevalidationDfd == null ? void 0 : pendingRevalidationDfd.resolve();
        pendingRevalidationDfd = null;
      }
      async function navigate(to, opts) {
        if (typeof to === "number") {
          init.history.go(to);
          return;
        }
        let normalizedPath = normalizeTo(
          state.location,
          state.matches,
          basename,
          to,
          opts == null ? void 0 : opts.fromRouteId,
          opts == null ? void 0 : opts.relative
        );
        let { path, submission, error } = normalizeNavigateOptions(
          false,
          normalizedPath,
          opts
        );
        let currentLocation = state.location;
        let nextLocation = createLocation(state.location, path, opts && opts.state);
        nextLocation = {
          ...nextLocation,
          ...init.history.encodeLocation(nextLocation)
        };
        let userReplace = opts && opts.replace != null ? opts.replace : void 0;
        let historyAction = "PUSH";
        if (userReplace === true) {
          historyAction = "REPLACE";
        } else if (userReplace === false) {
        } else if (submission != null && isMutationMethod(submission.formMethod) && submission.formAction === state.location.pathname + state.location.search) {
          historyAction = "REPLACE";
        }
        let preventScrollReset = opts && "preventScrollReset" in opts ? opts.preventScrollReset === true : void 0;
        let flushSync2 = (opts && opts.flushSync) === true;
        let blockerKey = shouldBlockNavigation({
          currentLocation,
          nextLocation,
          historyAction
        });
        if (blockerKey) {
          updateBlocker(blockerKey, {
            state: "blocked",
            location: nextLocation,
            proceed() {
              updateBlocker(blockerKey, {
                state: "proceeding",
                proceed: void 0,
                reset: void 0,
                location: nextLocation
              });
              navigate(to, opts);
            },
            reset() {
              let blockers = new Map(state.blockers);
              blockers.set(blockerKey, IDLE_BLOCKER);
              updateState({ blockers });
            }
          });
          return;
        }
        await startNavigation(historyAction, nextLocation, {
          submission,
          // Send through the formData serialization error if we have one so we can
          // render at the right error boundary after we match routes
          pendingError: error,
          preventScrollReset,
          replace: opts && opts.replace,
          enableViewTransition: opts && opts.viewTransition,
          flushSync: flushSync2
        });
      }
      function revalidate() {
        if (!pendingRevalidationDfd) {
          pendingRevalidationDfd = createDeferred();
        }
        interruptActiveLoads();
        updateState({ revalidation: "loading" });
        let promise = pendingRevalidationDfd.promise;
        if (state.navigation.state === "submitting") {
          return promise;
        }
        if (state.navigation.state === "idle") {
          startNavigation(state.historyAction, state.location, {
            startUninterruptedRevalidation: true
          });
          return promise;
        }
        startNavigation(
          pendingAction || state.historyAction,
          state.navigation.location,
          {
            overrideNavigation: state.navigation,
            // Proxy through any rending view transition
            enableViewTransition: pendingViewTransitionEnabled === true
          }
        );
        return promise;
      }
      async function startNavigation(historyAction, location, opts) {
        pendingNavigationController && pendingNavigationController.abort();
        pendingNavigationController = null;
        pendingAction = historyAction;
        isUninterruptedRevalidation = (opts && opts.startUninterruptedRevalidation) === true;
        saveScrollPosition(state.location, state.matches);
        pendingPreventScrollReset = (opts && opts.preventScrollReset) === true;
        pendingViewTransitionEnabled = (opts && opts.enableViewTransition) === true;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let loadingNavigation = opts && opts.overrideNavigation;
        let matches = (opts == null ? void 0 : opts.initialHydration) && state.matches && state.matches.length > 0 && !initialMatchesIsFOW ? (
          // `matchRoutes()` has already been called if we're in here via `router.initialize()`
          state.matches
        ) : matchRoutes(routesToUse, location, basename);
        let flushSync2 = (opts && opts.flushSync) === true;
        if (matches && state.initialized && !isRevalidationRequired && isHashChangeOnly(state.location, location) && !(opts && opts.submission && isMutationMethod(opts.submission.formMethod))) {
          completeNavigation(location, { matches }, { flushSync: flushSync2 });
          return;
        }
        let fogOfWar = checkFogOfWar(matches, routesToUse, location.pathname);
        if (fogOfWar.active && fogOfWar.matches) {
          matches = fogOfWar.matches;
        }
        if (!matches) {
          let { error, notFoundMatches, route } = handleNavigational404(
            location.pathname
          );
          completeNavigation(
            location,
            {
              matches: notFoundMatches,
              loaderData: {},
              errors: {
                [route.id]: error
              }
            },
            { flushSync: flushSync2 }
          );
          return;
        }
        pendingNavigationController = new AbortController();
        let request = createClientSideRequest(
          init.history,
          location,
          pendingNavigationController.signal,
          opts && opts.submission
        );
        let pendingActionResult;
        if (opts && opts.pendingError) {
          pendingActionResult = [
            findNearestBoundary(matches).route.id,
            { type: "error", error: opts.pendingError }
          ];
        } else if (opts && opts.submission && isMutationMethod(opts.submission.formMethod)) {
          let actionResult = await handleAction(
            request,
            location,
            opts.submission,
            matches,
            fogOfWar.active,
            { replace: opts.replace, flushSync: flushSync2 }
          );
          if (actionResult.shortCircuited) {
            return;
          }
          if (actionResult.pendingActionResult) {
            let [routeId, result] = actionResult.pendingActionResult;
            if (isErrorResult(result) && isRouteErrorResponse(result.error) && result.error.status === 404) {
              pendingNavigationController = null;
              completeNavigation(location, {
                matches: actionResult.matches,
                loaderData: {},
                errors: {
                  [routeId]: result.error
                }
              });
              return;
            }
          }
          matches = actionResult.matches || matches;
          pendingActionResult = actionResult.pendingActionResult;
          loadingNavigation = getLoadingNavigation(location, opts.submission);
          flushSync2 = false;
          fogOfWar.active = false;
          request = createClientSideRequest(
            init.history,
            request.url,
            request.signal
          );
        }
        let {
          shortCircuited,
          matches: updatedMatches,
          loaderData,
          errors
        } = await handleLoaders(
          request,
          location,
          matches,
          fogOfWar.active,
          loadingNavigation,
          opts && opts.submission,
          opts && opts.fetcherSubmission,
          opts && opts.replace,
          opts && opts.initialHydration === true,
          flushSync2,
          pendingActionResult
        );
        if (shortCircuited) {
          return;
        }
        pendingNavigationController = null;
        completeNavigation(location, {
          matches: updatedMatches || matches,
          ...getActionDataForCommit(pendingActionResult),
          loaderData,
          errors
        });
      }
      async function handleAction(request, location, submission, matches, isFogOfWar, opts = {}) {
        interruptActiveLoads();
        let navigation = getSubmittingNavigation(location, submission);
        updateState({ navigation }, { flushSync: opts.flushSync === true });
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            matches,
            location.pathname,
            request.signal
          );
          if (discoverResult.type === "aborted") {
            return { shortCircuited: true };
          } else if (discoverResult.type === "error") {
            let boundaryId = findNearestBoundary(discoverResult.partialMatches).route.id;
            return {
              matches: discoverResult.partialMatches,
              pendingActionResult: [
                boundaryId,
                {
                  type: "error",
                  error: discoverResult.error
                }
              ]
            };
          } else if (!discoverResult.matches) {
            let { notFoundMatches, error, route } = handleNavigational404(
              location.pathname
            );
            return {
              matches: notFoundMatches,
              pendingActionResult: [
                route.id,
                {
                  type: "error",
                  error
                }
              ]
            };
          } else {
            matches = discoverResult.matches;
          }
        }
        let result;
        let actionMatch = getTargetMatch(matches, location);
        if (!actionMatch.route.action && !actionMatch.route.lazy) {
          result = {
            type: "error",
            error: getInternalRouterError(405, {
              method: request.method,
              pathname: location.pathname,
              routeId: actionMatch.route.id
            })
          };
        } else {
          let results = await callDataStrategy(
            "action",
            state,
            request,
            [actionMatch],
            matches,
            null
          );
          result = results[actionMatch.route.id];
          if (request.signal.aborted) {
            return { shortCircuited: true };
          }
        }
        if (isRedirectResult(result)) {
          let replace2;
          if (opts && opts.replace != null) {
            replace2 = opts.replace;
          } else {
            let location2 = normalizeRedirectLocation(
              result.response.headers.get("Location"),
              new URL(request.url),
              basename
            );
            replace2 = location2 === state.location.pathname + state.location.search;
          }
          await startRedirectNavigation(request, result, true, {
            submission,
            replace: replace2
          });
          return { shortCircuited: true };
        }
        if (isErrorResult(result)) {
          let boundaryMatch = findNearestBoundary(matches, actionMatch.route.id);
          if ((opts && opts.replace) !== true) {
            pendingAction = "PUSH";
          }
          return {
            matches,
            pendingActionResult: [boundaryMatch.route.id, result]
          };
        }
        return {
          matches,
          pendingActionResult: [actionMatch.route.id, result]
        };
      }
      async function handleLoaders(request, location, matches, isFogOfWar, overrideNavigation, submission, fetcherSubmission, replace2, initialHydration, flushSync2, pendingActionResult) {
        let loadingNavigation = overrideNavigation || getLoadingNavigation(location, submission);
        let activeSubmission = submission || fetcherSubmission || getSubmissionFromNavigation(loadingNavigation);
        let shouldUpdateNavigationState = !isUninterruptedRevalidation && !initialHydration;
        if (isFogOfWar) {
          if (shouldUpdateNavigationState) {
            let actionData = getUpdatedActionData(pendingActionResult);
            updateState(
              {
                navigation: loadingNavigation,
                ...actionData !== void 0 ? { actionData } : {}
              },
              {
                flushSync: flushSync2
              }
            );
          }
          let discoverResult = await discoverRoutes(
            matches,
            location.pathname,
            request.signal
          );
          if (discoverResult.type === "aborted") {
            return { shortCircuited: true };
          } else if (discoverResult.type === "error") {
            let boundaryId = findNearestBoundary(discoverResult.partialMatches).route.id;
            return {
              matches: discoverResult.partialMatches,
              loaderData: {},
              errors: {
                [boundaryId]: discoverResult.error
              }
            };
          } else if (!discoverResult.matches) {
            let { error, notFoundMatches, route } = handleNavigational404(
              location.pathname
            );
            return {
              matches: notFoundMatches,
              loaderData: {},
              errors: {
                [route.id]: error
              }
            };
          } else {
            matches = discoverResult.matches;
          }
        }
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let [matchesToLoad, revalidatingFetchers] = getMatchesToLoad(
          init.history,
          state,
          matches,
          activeSubmission,
          location,
          initialHydration === true,
          isRevalidationRequired,
          cancelledFetcherLoads,
          fetchersQueuedForDeletion,
          fetchLoadMatches,
          fetchRedirectIds,
          routesToUse,
          basename,
          pendingActionResult
        );
        pendingNavigationLoadId = ++incrementingLoadId;
        if (matchesToLoad.length === 0 && revalidatingFetchers.length === 0) {
          let updatedFetchers2 = markFetchRedirectsDone();
          completeNavigation(
            location,
            {
              matches,
              loaderData: {},
              // Commit pending error if we're short circuiting
              errors: pendingActionResult && isErrorResult(pendingActionResult[1]) ? { [pendingActionResult[0]]: pendingActionResult[1].error } : null,
              ...getActionDataForCommit(pendingActionResult),
              ...updatedFetchers2 ? { fetchers: new Map(state.fetchers) } : {}
            },
            { flushSync: flushSync2 }
          );
          return { shortCircuited: true };
        }
        if (shouldUpdateNavigationState) {
          let updates = {};
          if (!isFogOfWar) {
            updates.navigation = loadingNavigation;
            let actionData = getUpdatedActionData(pendingActionResult);
            if (actionData !== void 0) {
              updates.actionData = actionData;
            }
          }
          if (revalidatingFetchers.length > 0) {
            updates.fetchers = getUpdatedRevalidatingFetchers(revalidatingFetchers);
          }
          updateState(updates, { flushSync: flushSync2 });
        }
        revalidatingFetchers.forEach((rf) => {
          abortFetcher(rf.key);
          if (rf.controller) {
            fetchControllers.set(rf.key, rf.controller);
          }
        });
        let abortPendingFetchRevalidations = () => revalidatingFetchers.forEach((f) => abortFetcher(f.key));
        if (pendingNavigationController) {
          pendingNavigationController.signal.addEventListener(
            "abort",
            abortPendingFetchRevalidations
          );
        }
        let { loaderResults, fetcherResults } = await callLoadersAndMaybeResolveData(
          state,
          matches,
          matchesToLoad,
          revalidatingFetchers,
          request
        );
        if (request.signal.aborted) {
          return { shortCircuited: true };
        }
        if (pendingNavigationController) {
          pendingNavigationController.signal.removeEventListener(
            "abort",
            abortPendingFetchRevalidations
          );
        }
        revalidatingFetchers.forEach((rf) => fetchControllers.delete(rf.key));
        let redirect2 = findRedirect(loaderResults);
        if (redirect2) {
          await startRedirectNavigation(request, redirect2.result, true, {
            replace: replace2
          });
          return { shortCircuited: true };
        }
        redirect2 = findRedirect(fetcherResults);
        if (redirect2) {
          fetchRedirectIds.add(redirect2.key);
          await startRedirectNavigation(request, redirect2.result, true, {
            replace: replace2
          });
          return { shortCircuited: true };
        }
        let { loaderData, errors } = processLoaderData(
          state,
          matches,
          loaderResults,
          pendingActionResult,
          revalidatingFetchers,
          fetcherResults
        );
        if (initialHydration && state.errors) {
          errors = { ...state.errors, ...errors };
        }
        let updatedFetchers = markFetchRedirectsDone();
        let didAbortFetchLoads = abortStaleFetchLoads(pendingNavigationLoadId);
        let shouldUpdateFetchers = updatedFetchers || didAbortFetchLoads || revalidatingFetchers.length > 0;
        return {
          matches,
          loaderData,
          errors,
          ...shouldUpdateFetchers ? { fetchers: new Map(state.fetchers) } : {}
        };
      }
      function getUpdatedActionData(pendingActionResult) {
        if (pendingActionResult && !isErrorResult(pendingActionResult[1])) {
          return {
            [pendingActionResult[0]]: pendingActionResult[1].data
          };
        } else if (state.actionData) {
          if (Object.keys(state.actionData).length === 0) {
            return null;
          } else {
            return state.actionData;
          }
        }
      }
      function getUpdatedRevalidatingFetchers(revalidatingFetchers) {
        revalidatingFetchers.forEach((rf) => {
          let fetcher = state.fetchers.get(rf.key);
          let revalidatingFetcher = getLoadingFetcher(
            void 0,
            fetcher ? fetcher.data : void 0
          );
          state.fetchers.set(rf.key, revalidatingFetcher);
        });
        return new Map(state.fetchers);
      }
      async function fetch2(key, routeId, href, opts) {
        abortFetcher(key);
        let flushSync2 = (opts && opts.flushSync) === true;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let normalizedPath = normalizeTo(
          state.location,
          state.matches,
          basename,
          href,
          routeId,
          opts == null ? void 0 : opts.relative
        );
        let matches = matchRoutes(routesToUse, normalizedPath, basename);
        let fogOfWar = checkFogOfWar(matches, routesToUse, normalizedPath);
        if (fogOfWar.active && fogOfWar.matches) {
          matches = fogOfWar.matches;
        }
        if (!matches) {
          setFetcherError(
            key,
            routeId,
            getInternalRouterError(404, { pathname: normalizedPath }),
            { flushSync: flushSync2 }
          );
          return;
        }
        let { path, submission, error } = normalizeNavigateOptions(
          true,
          normalizedPath,
          opts
        );
        if (error) {
          setFetcherError(key, routeId, error, { flushSync: flushSync2 });
          return;
        }
        let match = getTargetMatch(matches, path);
        let preventScrollReset = (opts && opts.preventScrollReset) === true;
        if (submission && isMutationMethod(submission.formMethod)) {
          await handleFetcherAction(
            key,
            routeId,
            path,
            match,
            matches,
            fogOfWar.active,
            flushSync2,
            preventScrollReset,
            submission
          );
          return;
        }
        fetchLoadMatches.set(key, { routeId, path });
        await handleFetcherLoader(
          key,
          routeId,
          path,
          match,
          matches,
          fogOfWar.active,
          flushSync2,
          preventScrollReset,
          submission
        );
      }
      async function handleFetcherAction(key, routeId, path, match, requestMatches, isFogOfWar, flushSync2, preventScrollReset, submission) {
        interruptActiveLoads();
        fetchLoadMatches.delete(key);
        function detectAndHandle405Error(m) {
          if (!m.route.action && !m.route.lazy) {
            let error = getInternalRouterError(405, {
              method: submission.formMethod,
              pathname: path,
              routeId
            });
            setFetcherError(key, routeId, error, { flushSync: flushSync2 });
            return true;
          }
          return false;
        }
        if (!isFogOfWar && detectAndHandle405Error(match)) {
          return;
        }
        let existingFetcher = state.fetchers.get(key);
        updateFetcherState(key, getSubmittingFetcher(submission, existingFetcher), {
          flushSync: flushSync2
        });
        let abortController = new AbortController();
        let fetchRequest = createClientSideRequest(
          init.history,
          path,
          abortController.signal,
          submission
        );
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            requestMatches,
            path,
            fetchRequest.signal
          );
          if (discoverResult.type === "aborted") {
            return;
          } else if (discoverResult.type === "error") {
            setFetcherError(key, routeId, discoverResult.error, { flushSync: flushSync2 });
            return;
          } else if (!discoverResult.matches) {
            setFetcherError(
              key,
              routeId,
              getInternalRouterError(404, { pathname: path }),
              { flushSync: flushSync2 }
            );
            return;
          } else {
            requestMatches = discoverResult.matches;
            match = getTargetMatch(requestMatches, path);
            if (detectAndHandle405Error(match)) {
              return;
            }
          }
        }
        fetchControllers.set(key, abortController);
        let originatingLoadId = incrementingLoadId;
        let actionResults = await callDataStrategy(
          "action",
          state,
          fetchRequest,
          [match],
          requestMatches,
          key
        );
        let actionResult = actionResults[match.route.id];
        if (fetchRequest.signal.aborted) {
          if (fetchControllers.get(key) === abortController) {
            fetchControllers.delete(key);
          }
          return;
        }
        if (fetchersQueuedForDeletion.has(key)) {
          if (isRedirectResult(actionResult) || isErrorResult(actionResult)) {
            updateFetcherState(key, getDoneFetcher(void 0));
            return;
          }
        } else {
          if (isRedirectResult(actionResult)) {
            fetchControllers.delete(key);
            if (pendingNavigationLoadId > originatingLoadId) {
              updateFetcherState(key, getDoneFetcher(void 0));
              return;
            } else {
              fetchRedirectIds.add(key);
              updateFetcherState(key, getLoadingFetcher(submission));
              return startRedirectNavigation(fetchRequest, actionResult, false, {
                fetcherSubmission: submission,
                preventScrollReset
              });
            }
          }
          if (isErrorResult(actionResult)) {
            setFetcherError(key, routeId, actionResult.error);
            return;
          }
        }
        let nextLocation = state.navigation.location || state.location;
        let revalidationRequest = createClientSideRequest(
          init.history,
          nextLocation,
          abortController.signal
        );
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let matches = state.navigation.state !== "idle" ? matchRoutes(routesToUse, state.navigation.location, basename) : state.matches;
        invariant(matches, "Didn't find any matches after fetcher action");
        let loadId = ++incrementingLoadId;
        fetchReloadIds.set(key, loadId);
        let loadFetcher = getLoadingFetcher(submission, actionResult.data);
        state.fetchers.set(key, loadFetcher);
        let [matchesToLoad, revalidatingFetchers] = getMatchesToLoad(
          init.history,
          state,
          matches,
          submission,
          nextLocation,
          false,
          isRevalidationRequired,
          cancelledFetcherLoads,
          fetchersQueuedForDeletion,
          fetchLoadMatches,
          fetchRedirectIds,
          routesToUse,
          basename,
          [match.route.id, actionResult]
        );
        revalidatingFetchers.filter((rf) => rf.key !== key).forEach((rf) => {
          let staleKey = rf.key;
          let existingFetcher2 = state.fetchers.get(staleKey);
          let revalidatingFetcher = getLoadingFetcher(
            void 0,
            existingFetcher2 ? existingFetcher2.data : void 0
          );
          state.fetchers.set(staleKey, revalidatingFetcher);
          abortFetcher(staleKey);
          if (rf.controller) {
            fetchControllers.set(staleKey, rf.controller);
          }
        });
        updateState({ fetchers: new Map(state.fetchers) });
        let abortPendingFetchRevalidations = () => revalidatingFetchers.forEach((rf) => abortFetcher(rf.key));
        abortController.signal.addEventListener(
          "abort",
          abortPendingFetchRevalidations
        );
        let { loaderResults, fetcherResults } = await callLoadersAndMaybeResolveData(
          state,
          matches,
          matchesToLoad,
          revalidatingFetchers,
          revalidationRequest
        );
        if (abortController.signal.aborted) {
          return;
        }
        abortController.signal.removeEventListener(
          "abort",
          abortPendingFetchRevalidations
        );
        fetchReloadIds.delete(key);
        fetchControllers.delete(key);
        revalidatingFetchers.forEach((r) => fetchControllers.delete(r.key));
        let redirect2 = findRedirect(loaderResults);
        if (redirect2) {
          return startRedirectNavigation(
            revalidationRequest,
            redirect2.result,
            false,
            { preventScrollReset }
          );
        }
        redirect2 = findRedirect(fetcherResults);
        if (redirect2) {
          fetchRedirectIds.add(redirect2.key);
          return startRedirectNavigation(
            revalidationRequest,
            redirect2.result,
            false,
            { preventScrollReset }
          );
        }
        let { loaderData, errors } = processLoaderData(
          state,
          matches,
          loaderResults,
          void 0,
          revalidatingFetchers,
          fetcherResults
        );
        if (state.fetchers.has(key)) {
          let doneFetcher = getDoneFetcher(actionResult.data);
          state.fetchers.set(key, doneFetcher);
        }
        abortStaleFetchLoads(loadId);
        if (state.navigation.state === "loading" && loadId > pendingNavigationLoadId) {
          invariant(pendingAction, "Expected pending action");
          pendingNavigationController && pendingNavigationController.abort();
          completeNavigation(state.navigation.location, {
            matches,
            loaderData,
            errors,
            fetchers: new Map(state.fetchers)
          });
        } else {
          updateState({
            errors,
            loaderData: mergeLoaderData(
              state.loaderData,
              loaderData,
              matches,
              errors
            ),
            fetchers: new Map(state.fetchers)
          });
          isRevalidationRequired = false;
        }
      }
      async function handleFetcherLoader(key, routeId, path, match, matches, isFogOfWar, flushSync2, preventScrollReset, submission) {
        let existingFetcher = state.fetchers.get(key);
        updateFetcherState(
          key,
          getLoadingFetcher(
            submission,
            existingFetcher ? existingFetcher.data : void 0
          ),
          { flushSync: flushSync2 }
        );
        let abortController = new AbortController();
        let fetchRequest = createClientSideRequest(
          init.history,
          path,
          abortController.signal
        );
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            matches,
            path,
            fetchRequest.signal
          );
          if (discoverResult.type === "aborted") {
            return;
          } else if (discoverResult.type === "error") {
            setFetcherError(key, routeId, discoverResult.error, { flushSync: flushSync2 });
            return;
          } else if (!discoverResult.matches) {
            setFetcherError(
              key,
              routeId,
              getInternalRouterError(404, { pathname: path }),
              { flushSync: flushSync2 }
            );
            return;
          } else {
            matches = discoverResult.matches;
            match = getTargetMatch(matches, path);
          }
        }
        fetchControllers.set(key, abortController);
        let originatingLoadId = incrementingLoadId;
        let results = await callDataStrategy(
          "loader",
          state,
          fetchRequest,
          [match],
          matches,
          key
        );
        let result = results[match.route.id];
        if (fetchControllers.get(key) === abortController) {
          fetchControllers.delete(key);
        }
        if (fetchRequest.signal.aborted) {
          return;
        }
        if (fetchersQueuedForDeletion.has(key)) {
          updateFetcherState(key, getDoneFetcher(void 0));
          return;
        }
        if (isRedirectResult(result)) {
          if (pendingNavigationLoadId > originatingLoadId) {
            updateFetcherState(key, getDoneFetcher(void 0));
            return;
          } else {
            fetchRedirectIds.add(key);
            await startRedirectNavigation(fetchRequest, result, false, {
              preventScrollReset
            });
            return;
          }
        }
        if (isErrorResult(result)) {
          setFetcherError(key, routeId, result.error);
          return;
        }
        updateFetcherState(key, getDoneFetcher(result.data));
      }
      async function startRedirectNavigation(request, redirect2, isNavigation, {
        submission,
        fetcherSubmission,
        preventScrollReset,
        replace: replace2
      } = {}) {
        if (redirect2.response.headers.has("X-Remix-Revalidate")) {
          isRevalidationRequired = true;
        }
        let location = redirect2.response.headers.get("Location");
        invariant(location, "Expected a Location header on the redirect Response");
        location = normalizeRedirectLocation(
          location,
          new URL(request.url),
          basename
        );
        let redirectLocation = createLocation(state.location, location, {
          _isRedirect: true
        });
        if (isBrowser) {
          let isDocumentReload = false;
          if (redirect2.response.headers.has("X-Remix-Reload-Document")) {
            isDocumentReload = true;
          } else if (ABSOLUTE_URL_REGEX.test(location)) {
            const url = init.history.createURL(location);
            isDocumentReload = // Hard reload if it's an absolute URL to a new origin
            url.origin !== routerWindow.location.origin || // Hard reload if it's an absolute URL that does not match our basename
            stripBasename(url.pathname, basename) == null;
          }
          if (isDocumentReload) {
            if (replace2) {
              routerWindow.location.replace(location);
            } else {
              routerWindow.location.assign(location);
            }
            return;
          }
        }
        pendingNavigationController = null;
        let redirectNavigationType = replace2 === true || redirect2.response.headers.has("X-Remix-Replace") ? "REPLACE" : "PUSH";
        let { formMethod, formAction, formEncType } = state.navigation;
        if (!submission && !fetcherSubmission && formMethod && formAction && formEncType) {
          submission = getSubmissionFromNavigation(state.navigation);
        }
        let activeSubmission = submission || fetcherSubmission;
        if (redirectPreserveMethodStatusCodes.has(redirect2.response.status) && activeSubmission && isMutationMethod(activeSubmission.formMethod)) {
          await startNavigation(redirectNavigationType, redirectLocation, {
            submission: {
              ...activeSubmission,
              formAction: location
            },
            // Preserve these flags across redirects
            preventScrollReset: preventScrollReset || pendingPreventScrollReset,
            enableViewTransition: isNavigation ? pendingViewTransitionEnabled : void 0
          });
        } else {
          let overrideNavigation = getLoadingNavigation(
            redirectLocation,
            submission
          );
          await startNavigation(redirectNavigationType, redirectLocation, {
            overrideNavigation,
            // Send fetcher submissions through for shouldRevalidate
            fetcherSubmission,
            // Preserve these flags across redirects
            preventScrollReset: preventScrollReset || pendingPreventScrollReset,
            enableViewTransition: isNavigation ? pendingViewTransitionEnabled : void 0
          });
        }
      }
      async function callDataStrategy(type, state2, request, matchesToLoad, matches, fetcherKey) {
        let results;
        let dataResults = {};
        try {
          results = await callDataStrategyImpl(
            dataStrategyImpl,
            type,
            state2,
            request,
            matchesToLoad,
            matches,
            fetcherKey,
            manifest,
            mapRouteProperties2
          );
        } catch (e) {
          matchesToLoad.forEach((m) => {
            dataResults[m.route.id] = {
              type: "error",
              error: e
            };
          });
          return dataResults;
        }
        for (let [routeId, result] of Object.entries(results)) {
          if (isRedirectDataStrategyResult(result)) {
            let response = result.result;
            dataResults[routeId] = {
              type: "redirect",
              response: normalizeRelativeRoutingRedirectResponse(
                response,
                request,
                routeId,
                matches,
                basename
              )
            };
          } else {
            dataResults[routeId] = await convertDataStrategyResultToDataResult(
              result
            );
          }
        }
        return dataResults;
      }
      async function callLoadersAndMaybeResolveData(state2, matches, matchesToLoad, fetchersToLoad, request) {
        let loaderResultsPromise = callDataStrategy(
          "loader",
          state2,
          request,
          matchesToLoad,
          matches,
          null
        );
        let fetcherResultsPromise = Promise.all(
          fetchersToLoad.map(async (f) => {
            if (f.matches && f.match && f.controller) {
              let results = await callDataStrategy(
                "loader",
                state2,
                createClientSideRequest(init.history, f.path, f.controller.signal),
                [f.match],
                f.matches,
                f.key
              );
              let result = results[f.match.route.id];
              return { [f.key]: result };
            } else {
              return Promise.resolve({
                [f.key]: {
                  type: "error",
                  error: getInternalRouterError(404, {
                    pathname: f.path
                  })
                }
              });
            }
          })
        );
        let loaderResults = await loaderResultsPromise;
        let fetcherResults = (await fetcherResultsPromise).reduce(
          (acc, r) => Object.assign(acc, r),
          {}
        );
        return {
          loaderResults,
          fetcherResults
        };
      }
      function interruptActiveLoads() {
        isRevalidationRequired = true;
        fetchLoadMatches.forEach((_, key) => {
          if (fetchControllers.has(key)) {
            cancelledFetcherLoads.add(key);
          }
          abortFetcher(key);
        });
      }
      function updateFetcherState(key, fetcher, opts = {}) {
        state.fetchers.set(key, fetcher);
        updateState(
          { fetchers: new Map(state.fetchers) },
          { flushSync: (opts && opts.flushSync) === true }
        );
      }
      function setFetcherError(key, routeId, error, opts = {}) {
        let boundaryMatch = findNearestBoundary(state.matches, routeId);
        deleteFetcher(key);
        updateState(
          {
            errors: {
              [boundaryMatch.route.id]: error
            },
            fetchers: new Map(state.fetchers)
          },
          { flushSync: (opts && opts.flushSync) === true }
        );
      }
      function getFetcher(key) {
        activeFetchers.set(key, (activeFetchers.get(key) || 0) + 1);
        if (fetchersQueuedForDeletion.has(key)) {
          fetchersQueuedForDeletion.delete(key);
        }
        return state.fetchers.get(key) || IDLE_FETCHER;
      }
      function deleteFetcher(key) {
        let fetcher = state.fetchers.get(key);
        if (fetchControllers.has(key) && !(fetcher && fetcher.state === "loading" && fetchReloadIds.has(key))) {
          abortFetcher(key);
        }
        fetchLoadMatches.delete(key);
        fetchReloadIds.delete(key);
        fetchRedirectIds.delete(key);
        fetchersQueuedForDeletion.delete(key);
        cancelledFetcherLoads.delete(key);
        state.fetchers.delete(key);
      }
      function queueFetcherForDeletion(key) {
        let count = (activeFetchers.get(key) || 0) - 1;
        if (count <= 0) {
          activeFetchers.delete(key);
          fetchersQueuedForDeletion.add(key);
        } else {
          activeFetchers.set(key, count);
        }
        updateState({ fetchers: new Map(state.fetchers) });
      }
      function abortFetcher(key) {
        let controller = fetchControllers.get(key);
        if (controller) {
          controller.abort();
          fetchControllers.delete(key);
        }
      }
      function markFetchersDone(keys) {
        for (let key of keys) {
          let fetcher = getFetcher(key);
          let doneFetcher = getDoneFetcher(fetcher.data);
          state.fetchers.set(key, doneFetcher);
        }
      }
      function markFetchRedirectsDone() {
        let doneKeys = [];
        let updatedFetchers = false;
        for (let key of fetchRedirectIds) {
          let fetcher = state.fetchers.get(key);
          invariant(fetcher, `Expected fetcher: ${key}`);
          if (fetcher.state === "loading") {
            fetchRedirectIds.delete(key);
            doneKeys.push(key);
            updatedFetchers = true;
          }
        }
        markFetchersDone(doneKeys);
        return updatedFetchers;
      }
      function abortStaleFetchLoads(landedId) {
        let yeetedKeys = [];
        for (let [key, id] of fetchReloadIds) {
          if (id < landedId) {
            let fetcher = state.fetchers.get(key);
            invariant(fetcher, `Expected fetcher: ${key}`);
            if (fetcher.state === "loading") {
              abortFetcher(key);
              fetchReloadIds.delete(key);
              yeetedKeys.push(key);
            }
          }
        }
        markFetchersDone(yeetedKeys);
        return yeetedKeys.length > 0;
      }
      function getBlocker(key, fn) {
        let blocker = state.blockers.get(key) || IDLE_BLOCKER;
        if (blockerFunctions.get(key) !== fn) {
          blockerFunctions.set(key, fn);
        }
        return blocker;
      }
      function deleteBlocker(key) {
        state.blockers.delete(key);
        blockerFunctions.delete(key);
      }
      function updateBlocker(key, newBlocker) {
        let blocker = state.blockers.get(key) || IDLE_BLOCKER;
        invariant(
          blocker.state === "unblocked" && newBlocker.state === "blocked" || blocker.state === "blocked" && newBlocker.state === "blocked" || blocker.state === "blocked" && newBlocker.state === "proceeding" || blocker.state === "blocked" && newBlocker.state === "unblocked" || blocker.state === "proceeding" && newBlocker.state === "unblocked",
          `Invalid blocker state transition: ${blocker.state} -> ${newBlocker.state}`
        );
        let blockers = new Map(state.blockers);
        blockers.set(key, newBlocker);
        updateState({ blockers });
      }
      function shouldBlockNavigation({
        currentLocation,
        nextLocation,
        historyAction
      }) {
        if (blockerFunctions.size === 0) {
          return;
        }
        if (blockerFunctions.size > 1) {
          warning(false, "A router only supports one blocker at a time");
        }
        let entries = Array.from(blockerFunctions.entries());
        let [blockerKey, blockerFunction] = entries[entries.length - 1];
        let blocker = state.blockers.get(blockerKey);
        if (blocker && blocker.state === "proceeding") {
          return;
        }
        if (blockerFunction({ currentLocation, nextLocation, historyAction })) {
          return blockerKey;
        }
      }
      function handleNavigational404(pathname) {
        let error = getInternalRouterError(404, { pathname });
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let { matches, route } = getShortCircuitMatches(routesToUse);
        return { notFoundMatches: matches, route, error };
      }
      function enableScrollRestoration(positions, getPosition, getKey) {
        savedScrollPositions = positions;
        getScrollPosition = getPosition;
        getScrollRestorationKey = getKey || null;
        if (!initialScrollRestored && state.navigation === IDLE_NAVIGATION) {
          initialScrollRestored = true;
          let y = getSavedScrollPosition(state.location, state.matches);
          if (y != null) {
            updateState({ restoreScrollPosition: y });
          }
        }
        return () => {
          savedScrollPositions = null;
          getScrollPosition = null;
          getScrollRestorationKey = null;
        };
      }
      function getScrollKey(location, matches) {
        if (getScrollRestorationKey) {
          let key = getScrollRestorationKey(
            location,
            matches.map((m) => convertRouteMatchToUiMatch(m, state.loaderData))
          );
          return key || location.key;
        }
        return location.key;
      }
      function saveScrollPosition(location, matches) {
        if (savedScrollPositions && getScrollPosition) {
          let key = getScrollKey(location, matches);
          savedScrollPositions[key] = getScrollPosition();
        }
      }
      function getSavedScrollPosition(location, matches) {
        if (savedScrollPositions) {
          let key = getScrollKey(location, matches);
          let y = savedScrollPositions[key];
          if (typeof y === "number") {
            return y;
          }
        }
        return null;
      }
      function checkFogOfWar(matches, routesToUse, pathname) {
        if (patchRoutesOnNavigationImpl) {
          if (!matches) {
            let fogMatches = matchRoutesImpl(
              routesToUse,
              pathname,
              basename,
              true
            );
            return { active: true, matches: fogMatches || [] };
          } else {
            if (Object.keys(matches[0].params).length > 0) {
              let partialMatches = matchRoutesImpl(
                routesToUse,
                pathname,
                basename,
                true
              );
              return { active: true, matches: partialMatches };
            }
          }
        }
        return { active: false, matches: null };
      }
      async function discoverRoutes(matches, pathname, signal) {
        if (!patchRoutesOnNavigationImpl) {
          return { type: "success", matches };
        }
        let partialMatches = matches;
        while (true) {
          let isNonHMR = inFlightDataRoutes == null;
          let routesToUse = inFlightDataRoutes || dataRoutes;
          let localManifest = manifest;
          try {
            await patchRoutesOnNavigationImpl({
              signal,
              path: pathname,
              matches: partialMatches,
              patch: (routeId, children) => {
                if (signal.aborted) return;
                patchRoutesImpl(
                  routeId,
                  children,
                  routesToUse,
                  localManifest,
                  mapRouteProperties2
                );
              }
            });
          } catch (e) {
            return { type: "error", error: e, partialMatches };
          } finally {
            if (isNonHMR && !signal.aborted) {
              dataRoutes = [...dataRoutes];
            }
          }
          if (signal.aborted) {
            return { type: "aborted" };
          }
          let newMatches = matchRoutes(routesToUse, pathname, basename);
          if (newMatches) {
            return { type: "success", matches: newMatches };
          }
          let newPartialMatches = matchRoutesImpl(
            routesToUse,
            pathname,
            basename,
            true
          );
          if (!newPartialMatches || partialMatches.length === newPartialMatches.length && partialMatches.every(
            (m, i) => m.route.id === newPartialMatches[i].route.id
          )) {
            return { type: "success", matches: null };
          }
          partialMatches = newPartialMatches;
        }
      }
      function _internalSetRoutes(newRoutes) {
        manifest = {};
        inFlightDataRoutes = convertRoutesToDataRoutes(
          newRoutes,
          mapRouteProperties2,
          void 0,
          manifest
        );
      }
      function patchRoutes(routeId, children) {
        let isNonHMR = inFlightDataRoutes == null;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        patchRoutesImpl(
          routeId,
          children,
          routesToUse,
          manifest,
          mapRouteProperties2
        );
        if (isNonHMR) {
          dataRoutes = [...dataRoutes];
          updateState({});
        }
      }
      router2 = {
        get basename() {
          return basename;
        },
        get future() {
          return future;
        },
        get state() {
          return state;
        },
        get routes() {
          return dataRoutes;
        },
        get window() {
          return routerWindow;
        },
        initialize,
        subscribe,
        enableScrollRestoration,
        navigate,
        fetch: fetch2,
        revalidate,
        // Passthrough to history-aware createHref used by useHref so we get proper
        // hash-aware URLs in DOM paths
        createHref: (to) => init.history.createHref(to),
        encodeLocation: (to) => init.history.encodeLocation(to),
        getFetcher,
        deleteFetcher: queueFetcherForDeletion,
        dispose,
        getBlocker,
        deleteBlocker,
        patchRoutes,
        _internalFetchControllers: fetchControllers,
        // TODO: Remove setRoutes, it's temporary to avoid dealing with
        // updating the tree while validating the update algorithm.
        _internalSetRoutes
      };
      return router2;
    }
    function isSubmissionNavigation(opts) {
      return opts != null && ("formData" in opts && opts.formData != null || "body" in opts && opts.body !== void 0);
    }
    function normalizeTo(location, matches, basename, to, fromRouteId, relative) {
      let contextualMatches;
      let activeRouteMatch;
      if (fromRouteId) {
        contextualMatches = [];
        for (let match of matches) {
          contextualMatches.push(match);
          if (match.route.id === fromRouteId) {
            activeRouteMatch = match;
            break;
          }
        }
      } else {
        contextualMatches = matches;
        activeRouteMatch = matches[matches.length - 1];
      }
      let path = resolveTo(
        to ? to : ".",
        getResolveToMatches(contextualMatches),
        stripBasename(location.pathname, basename) || location.pathname,
        relative === "path"
      );
      if (to == null) {
        path.search = location.search;
        path.hash = location.hash;
      }
      if ((to == null || to === "" || to === ".") && activeRouteMatch) {
        let nakedIndex = hasNakedIndexQuery(path.search);
        if (activeRouteMatch.route.index && !nakedIndex) {
          path.search = path.search ? path.search.replace(/^\?/, "?index&") : "?index";
        } else if (!activeRouteMatch.route.index && nakedIndex) {
          let params = new URLSearchParams(path.search);
          let indexValues = params.getAll("index");
          params.delete("index");
          indexValues.filter((v) => v).forEach((v) => params.append("index", v));
          let qs = params.toString();
          path.search = qs ? `?${qs}` : "";
        }
      }
      if (basename !== "/") {
        path.pathname = path.pathname === "/" ? basename : joinPaths([basename, path.pathname]);
      }
      return createPath(path);
    }
    function normalizeNavigateOptions(isFetcher, path, opts) {
      if (!opts || !isSubmissionNavigation(opts)) {
        return { path };
      }
      if (opts.formMethod && !isValidMethod(opts.formMethod)) {
        return {
          path,
          error: getInternalRouterError(405, { method: opts.formMethod })
        };
      }
      let getInvalidBodyError = () => ({
        path,
        error: getInternalRouterError(400, { type: "invalid-body" })
      });
      let rawFormMethod = opts.formMethod || "get";
      let formMethod = rawFormMethod.toUpperCase();
      let formAction = stripHashFromPath(path);
      if (opts.body !== void 0) {
        if (opts.formEncType === "text/plain") {
          if (!isMutationMethod(formMethod)) {
            return getInvalidBodyError();
          }
          let text = typeof opts.body === "string" ? opts.body : opts.body instanceof FormData || opts.body instanceof URLSearchParams ? (
            // https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#plain-text-form-data
            Array.from(opts.body.entries()).reduce(
              (acc, [name, value]) => `${acc}${name}=${value}
`,
              ""
            )
          ) : String(opts.body);
          return {
            path,
            submission: {
              formMethod,
              formAction,
              formEncType: opts.formEncType,
              formData: void 0,
              json: void 0,
              text
            }
          };
        } else if (opts.formEncType === "application/json") {
          if (!isMutationMethod(formMethod)) {
            return getInvalidBodyError();
          }
          try {
            let json = typeof opts.body === "string" ? JSON.parse(opts.body) : opts.body;
            return {
              path,
              submission: {
                formMethod,
                formAction,
                formEncType: opts.formEncType,
                formData: void 0,
                json,
                text: void 0
              }
            };
          } catch (e) {
            return getInvalidBodyError();
          }
        }
      }
      invariant(
        typeof FormData === "function",
        "FormData is not available in this environment"
      );
      let searchParams;
      let formData;
      if (opts.formData) {
        searchParams = convertFormDataToSearchParams(opts.formData);
        formData = opts.formData;
      } else if (opts.body instanceof FormData) {
        searchParams = convertFormDataToSearchParams(opts.body);
        formData = opts.body;
      } else if (opts.body instanceof URLSearchParams) {
        searchParams = opts.body;
        formData = convertSearchParamsToFormData(searchParams);
      } else if (opts.body == null) {
        searchParams = new URLSearchParams();
        formData = new FormData();
      } else {
        try {
          searchParams = new URLSearchParams(opts.body);
          formData = convertSearchParamsToFormData(searchParams);
        } catch (e) {
          return getInvalidBodyError();
        }
      }
      let submission = {
        formMethod,
        formAction,
        formEncType: opts && opts.formEncType || "application/x-www-form-urlencoded",
        formData,
        json: void 0,
        text: void 0
      };
      if (isMutationMethod(submission.formMethod)) {
        return { path, submission };
      }
      let parsedPath = parsePath(path);
      if (isFetcher && parsedPath.search && hasNakedIndexQuery(parsedPath.search)) {
        searchParams.append("index", "");
      }
      parsedPath.search = `?${searchParams}`;
      return { path: createPath(parsedPath), submission };
    }
    function getLoaderMatchesUntilBoundary(matches, boundaryId, includeBoundary = false) {
      let index = matches.findIndex((m) => m.route.id === boundaryId);
      if (index >= 0) {
        return matches.slice(0, includeBoundary ? index + 1 : index);
      }
      return matches;
    }
    function getMatchesToLoad(history, state, matches, submission, location, initialHydration, isRevalidationRequired, cancelledFetcherLoads, fetchersQueuedForDeletion, fetchLoadMatches, fetchRedirectIds, routesToUse, basename, pendingActionResult) {
      let actionResult = pendingActionResult ? isErrorResult(pendingActionResult[1]) ? pendingActionResult[1].error : pendingActionResult[1].data : void 0;
      let currentUrl = history.createURL(state.location);
      let nextUrl = history.createURL(location);
      let boundaryMatches = matches;
      if (initialHydration && state.errors) {
        boundaryMatches = getLoaderMatchesUntilBoundary(
          matches,
          Object.keys(state.errors)[0],
          true
        );
      } else if (pendingActionResult && isErrorResult(pendingActionResult[1])) {
        boundaryMatches = getLoaderMatchesUntilBoundary(
          matches,
          pendingActionResult[0]
        );
      }
      let actionStatus = pendingActionResult ? pendingActionResult[1].statusCode : void 0;
      let shouldSkipRevalidation = actionStatus && actionStatus >= 400;
      let navigationMatches = boundaryMatches.filter((match, index) => {
        let { route } = match;
        if (route.lazy) {
          return true;
        }
        if (route.loader == null) {
          return false;
        }
        if (initialHydration) {
          return shouldLoadRouteOnHydration(route, state.loaderData, state.errors);
        }
        if (isNewLoader(state.loaderData, state.matches[index], match)) {
          return true;
        }
        let currentRouteMatch = state.matches[index];
        let nextRouteMatch = match;
        return shouldRevalidateLoader(match, {
          currentUrl,
          currentParams: currentRouteMatch.params,
          nextUrl,
          nextParams: nextRouteMatch.params,
          ...submission,
          actionResult,
          actionStatus,
          defaultShouldRevalidate: shouldSkipRevalidation ? false : (
            // Forced revalidation due to submission, useRevalidator, or X-Remix-Revalidate
            isRevalidationRequired || currentUrl.pathname + currentUrl.search === nextUrl.pathname + nextUrl.search || // Search params affect all loaders
            currentUrl.search !== nextUrl.search || isNewRouteInstance(currentRouteMatch, nextRouteMatch)
          )
        });
      });
      let revalidatingFetchers = [];
      fetchLoadMatches.forEach((f, key) => {
        if (initialHydration || !matches.some((m) => m.route.id === f.routeId) || fetchersQueuedForDeletion.has(key)) {
          return;
        }
        let fetcherMatches = matchRoutes(routesToUse, f.path, basename);
        if (!fetcherMatches) {
          revalidatingFetchers.push({
            key,
            routeId: f.routeId,
            path: f.path,
            matches: null,
            match: null,
            controller: null
          });
          return;
        }
        let fetcher = state.fetchers.get(key);
        let fetcherMatch = getTargetMatch(fetcherMatches, f.path);
        let shouldRevalidate = false;
        if (fetchRedirectIds.has(key)) {
          shouldRevalidate = false;
        } else if (cancelledFetcherLoads.has(key)) {
          cancelledFetcherLoads.delete(key);
          shouldRevalidate = true;
        } else if (fetcher && fetcher.state !== "idle" && fetcher.data === void 0) {
          shouldRevalidate = isRevalidationRequired;
        } else {
          shouldRevalidate = shouldRevalidateLoader(fetcherMatch, {
            currentUrl,
            currentParams: state.matches[state.matches.length - 1].params,
            nextUrl,
            nextParams: matches[matches.length - 1].params,
            ...submission,
            actionResult,
            actionStatus,
            defaultShouldRevalidate: shouldSkipRevalidation ? false : isRevalidationRequired
          });
        }
        if (shouldRevalidate) {
          revalidatingFetchers.push({
            key,
            routeId: f.routeId,
            path: f.path,
            matches: fetcherMatches,
            match: fetcherMatch,
            controller: new AbortController()
          });
        }
      });
      return [navigationMatches, revalidatingFetchers];
    }
    function shouldLoadRouteOnHydration(route, loaderData, errors) {
      if (route.lazy) {
        return true;
      }
      if (!route.loader) {
        return false;
      }
      let hasData = loaderData != null && loaderData[route.id] !== void 0;
      let hasError = errors != null && errors[route.id] !== void 0;
      if (!hasData && hasError) {
        return false;
      }
      if (typeof route.loader === "function" && route.loader.hydrate === true) {
        return true;
      }
      return !hasData && !hasError;
    }
    function isNewLoader(currentLoaderData, currentMatch, match) {
      let isNew = (
        // [a] -> [a, b]
        !currentMatch || // [a, b] -> [a, c]
        match.route.id !== currentMatch.route.id
      );
      let isMissingData = !currentLoaderData.hasOwnProperty(match.route.id);
      return isNew || isMissingData;
    }
    function isNewRouteInstance(currentMatch, match) {
      let currentPath = currentMatch.route.path;
      return (
        // param change for this match, /users/123 -> /users/456
        currentMatch.pathname !== match.pathname || // splat param changed, which is not present in match.path
        // e.g. /files/images/avatar.jpg -> files/finances.xls
        currentPath != null && currentPath.endsWith("*") && currentMatch.params["*"] !== match.params["*"]
      );
    }
    function shouldRevalidateLoader(loaderMatch, arg) {
      if (loaderMatch.route.shouldRevalidate) {
        let routeChoice = loaderMatch.route.shouldRevalidate(arg);
        if (typeof routeChoice === "boolean") {
          return routeChoice;
        }
      }
      return arg.defaultShouldRevalidate;
    }
    function patchRoutesImpl(routeId, children, routesToUse, manifest, mapRouteProperties2) {
      let childrenToPatch;
      if (routeId) {
        let route = manifest[routeId];
        invariant(
          route,
          `No route found to patch children into: routeId = ${routeId}`
        );
        if (!route.children) {
          route.children = [];
        }
        childrenToPatch = route.children;
      } else {
        childrenToPatch = routesToUse;
      }
      let uniqueChildren = children.filter(
        (newRoute) => !childrenToPatch.some(
          (existingRoute) => isSameRoute(newRoute, existingRoute)
        )
      );
      let newRoutes = convertRoutesToDataRoutes(
        uniqueChildren,
        mapRouteProperties2,
        [routeId || "_", "patch", String((childrenToPatch == null ? void 0 : childrenToPatch.length) || "0")],
        manifest
      );
      childrenToPatch.push(...newRoutes);
    }
    function isSameRoute(newRoute, existingRoute) {
      if ("id" in newRoute && "id" in existingRoute && newRoute.id === existingRoute.id) {
        return true;
      }
      if (!(newRoute.index === existingRoute.index && newRoute.path === existingRoute.path && newRoute.caseSensitive === existingRoute.caseSensitive)) {
        return false;
      }
      if ((!newRoute.children || newRoute.children.length === 0) && (!existingRoute.children || existingRoute.children.length === 0)) {
        return true;
      }
      return newRoute.children.every(
        (aChild, i) => {
          var _a;
          return (_a = existingRoute.children) == null ? void 0 : _a.some((bChild) => isSameRoute(aChild, bChild));
        }
      );
    }
    async function loadLazyRouteModule(route, mapRouteProperties2, manifest) {
      if (!route.lazy) {
        return;
      }
      let lazyRoute = await route.lazy();
      if (!route.lazy) {
        return;
      }
      let routeToUpdate = manifest[route.id];
      invariant(routeToUpdate, "No route found in manifest");
      let routeUpdates = {};
      for (let lazyRouteProperty in lazyRoute) {
        let staticRouteValue = routeToUpdate[lazyRouteProperty];
        let isPropertyStaticallyDefined = staticRouteValue !== void 0 && // This property isn't static since it should always be updated based
        // on the route updates
        lazyRouteProperty !== "hasErrorBoundary";
        warning(
          !isPropertyStaticallyDefined,
          `Route "${routeToUpdate.id}" has a static property "${lazyRouteProperty}" defined but its lazy function is also returning a value for this property. The lazy route property "${lazyRouteProperty}" will be ignored.`
        );
        if (!isPropertyStaticallyDefined && !immutableRouteKeys.has(lazyRouteProperty)) {
          routeUpdates[lazyRouteProperty] = lazyRoute[lazyRouteProperty];
        }
      }
      Object.assign(routeToUpdate, routeUpdates);
      Object.assign(routeToUpdate, {
        // To keep things framework agnostic, we use the provided `mapRouteProperties`
        // function to set the framework-aware properties (`element`/`hasErrorBoundary`)
        // since the logic will differ between frameworks.
        ...mapRouteProperties2(routeToUpdate),
        lazy: void 0
      });
    }
    async function defaultDataStrategy({
      matches
    }) {
      let matchesToLoad = matches.filter((m) => m.shouldLoad);
      let results = await Promise.all(matchesToLoad.map((m) => m.resolve()));
      return results.reduce(
        (acc, result, i) => Object.assign(acc, { [matchesToLoad[i].route.id]: result }),
        {}
      );
    }
    async function callDataStrategyImpl(dataStrategyImpl, type, state, request, matchesToLoad, matches, fetcherKey, manifest, mapRouteProperties2, requestContext) {
      let loadRouteDefinitionsPromises = matches.map(
        (m) => m.route.lazy ? loadLazyRouteModule(m.route, mapRouteProperties2, manifest) : void 0
      );
      let dsMatches = matches.map((match, i) => {
        let loadRoutePromise = loadRouteDefinitionsPromises[i];
        let shouldLoad = matchesToLoad.some((m) => m.route.id === match.route.id);
        let resolve = async (handlerOverride) => {
          if (handlerOverride && request.method === "GET" && (match.route.lazy || match.route.loader)) {
            shouldLoad = true;
          }
          return shouldLoad ? callLoaderOrAction(
            type,
            request,
            match,
            loadRoutePromise,
            handlerOverride,
            requestContext
          ) : Promise.resolve({ type: "data", result: void 0 });
        };
        return {
          ...match,
          shouldLoad,
          resolve
        };
      });
      let results = await dataStrategyImpl({
        matches: dsMatches,
        request,
        params: matches[0].params,
        fetcherKey,
        context: requestContext
      });
      try {
        await Promise.all(loadRouteDefinitionsPromises);
      } catch (e) {
      }
      return results;
    }
    async function callLoaderOrAction(type, request, match, loadRoutePromise, handlerOverride, staticContext) {
      let result;
      let onReject;
      let runHandler = (handler) => {
        let reject;
        let abortPromise = new Promise((_, r) => reject = r);
        onReject = () => reject();
        request.signal.addEventListener("abort", onReject);
        let actualHandler = (ctx) => {
          if (typeof handler !== "function") {
            return Promise.reject(
              new Error(
                `You cannot call the handler for a route which defines a boolean "${type}" [routeId: ${match.route.id}]`
              )
            );
          }
          return handler(
            {
              request,
              params: match.params,
              context: staticContext
            },
            ...ctx !== void 0 ? [ctx] : []
          );
        };
        let handlerPromise = (async () => {
          try {
            let val = await (handlerOverride ? handlerOverride((ctx) => actualHandler(ctx)) : actualHandler());
            return { type: "data", result: val };
          } catch (e) {
            return { type: "error", result: e };
          }
        })();
        return Promise.race([handlerPromise, abortPromise]);
      };
      try {
        let handler = match.route[type];
        if (loadRoutePromise) {
          if (handler) {
            let handlerError;
            let [value] = await Promise.all([
              // If the handler throws, don't let it immediately bubble out,
              // since we need to let the lazy() execution finish so we know if this
              // route has a boundary that can handle the error
              runHandler(handler).catch((e) => {
                handlerError = e;
              }),
              loadRoutePromise
            ]);
            if (handlerError !== void 0) {
              throw handlerError;
            }
            result = value;
          } else {
            await loadRoutePromise;
            handler = match.route[type];
            if (handler) {
              result = await runHandler(handler);
            } else if (type === "action") {
              let url = new URL(request.url);
              let pathname = url.pathname + url.search;
              throw getInternalRouterError(405, {
                method: request.method,
                pathname,
                routeId: match.route.id
              });
            } else {
              return { type: "data", result: void 0 };
            }
          }
        } else if (!handler) {
          let url = new URL(request.url);
          let pathname = url.pathname + url.search;
          throw getInternalRouterError(404, {
            pathname
          });
        } else {
          result = await runHandler(handler);
        }
      } catch (e) {
        return { type: "error", result: e };
      } finally {
        if (onReject) {
          request.signal.removeEventListener("abort", onReject);
        }
      }
      return result;
    }
    async function convertDataStrategyResultToDataResult(dataStrategyResult) {
      var _a, _b, _c, _d, _e, _f;
      let { result, type } = dataStrategyResult;
      if (isResponse(result)) {
        let data2;
        try {
          let contentType = result.headers.get("Content-Type");
          if (contentType && /\bapplication\/json\b/.test(contentType)) {
            if (result.body == null) {
              data2 = null;
            } else {
              data2 = await result.json();
            }
          } else {
            data2 = await result.text();
          }
        } catch (e) {
          return { type: "error", error: e };
        }
        if (type === "error") {
          return {
            type: "error",
            error: new ErrorResponseImpl(result.status, result.statusText, data2),
            statusCode: result.status,
            headers: result.headers
          };
        }
        return {
          type: "data",
          data: data2,
          statusCode: result.status,
          headers: result.headers
        };
      }
      if (type === "error") {
        if (isDataWithResponseInit(result)) {
          if (result.data instanceof Error) {
            return {
              type: "error",
              error: result.data,
              statusCode: (_a = result.init) == null ? void 0 : _a.status,
              headers: ((_b = result.init) == null ? void 0 : _b.headers) ? new Headers(result.init.headers) : void 0
            };
          }
          return {
            type: "error",
            error: new ErrorResponseImpl(
              ((_c = result.init) == null ? void 0 : _c.status) || 500,
              void 0,
              result.data
            ),
            statusCode: isRouteErrorResponse(result) ? result.status : void 0,
            headers: ((_d = result.init) == null ? void 0 : _d.headers) ? new Headers(result.init.headers) : void 0
          };
        }
        return {
          type: "error",
          error: result,
          statusCode: isRouteErrorResponse(result) ? result.status : void 0
        };
      }
      if (isDataWithResponseInit(result)) {
        return {
          type: "data",
          data: result.data,
          statusCode: (_e = result.init) == null ? void 0 : _e.status,
          headers: ((_f = result.init) == null ? void 0 : _f.headers) ? new Headers(result.init.headers) : void 0
        };
      }
      return { type: "data", data: result };
    }
    function normalizeRelativeRoutingRedirectResponse(response, request, routeId, matches, basename) {
      let location = response.headers.get("Location");
      invariant(
        location,
        "Redirects returned/thrown from loaders/actions must have a Location header"
      );
      if (!ABSOLUTE_URL_REGEX.test(location)) {
        let trimmedMatches = matches.slice(
          0,
          matches.findIndex((m) => m.route.id === routeId) + 1
        );
        location = normalizeTo(
          new URL(request.url),
          trimmedMatches,
          basename,
          location
        );
        response.headers.set("Location", location);
      }
      return response;
    }
    function normalizeRedirectLocation(location, currentUrl, basename) {
      if (ABSOLUTE_URL_REGEX.test(location)) {
        let normalizedLocation = location;
        let url = normalizedLocation.startsWith("//") ? new URL(currentUrl.protocol + normalizedLocation) : new URL(normalizedLocation);
        let isSameBasename = stripBasename(url.pathname, basename) != null;
        if (url.origin === currentUrl.origin && isSameBasename) {
          return url.pathname + url.search + url.hash;
        }
      }
      return location;
    }
    function createClientSideRequest(history, location, signal, submission) {
      let url = history.createURL(stripHashFromPath(location)).toString();
      let init = { signal };
      if (submission && isMutationMethod(submission.formMethod)) {
        let { formMethod, formEncType } = submission;
        init.method = formMethod.toUpperCase();
        if (formEncType === "application/json") {
          init.headers = new Headers({ "Content-Type": formEncType });
          init.body = JSON.stringify(submission.json);
        } else if (formEncType === "text/plain") {
          init.body = submission.text;
        } else if (formEncType === "application/x-www-form-urlencoded" && submission.formData) {
          init.body = convertFormDataToSearchParams(submission.formData);
        } else {
          init.body = submission.formData;
        }
      }
      return new Request(url, init);
    }
    function convertFormDataToSearchParams(formData) {
      let searchParams = new URLSearchParams();
      for (let [key, value] of formData.entries()) {
        searchParams.append(key, typeof value === "string" ? value : value.name);
      }
      return searchParams;
    }
    function convertSearchParamsToFormData(searchParams) {
      let formData = new FormData();
      for (let [key, value] of searchParams.entries()) {
        formData.append(key, value);
      }
      return formData;
    }
    function processRouteLoaderData(matches, results, pendingActionResult, isStaticHandler = false, skipLoaderErrorBubbling = false) {
      let loaderData = {};
      let errors = null;
      let statusCode;
      let foundError = false;
      let loaderHeaders = {};
      let pendingError = pendingActionResult && isErrorResult(pendingActionResult[1]) ? pendingActionResult[1].error : void 0;
      matches.forEach((match) => {
        if (!(match.route.id in results)) {
          return;
        }
        let id = match.route.id;
        let result = results[id];
        invariant(
          !isRedirectResult(result),
          "Cannot handle redirect results in processLoaderData"
        );
        if (isErrorResult(result)) {
          let error = result.error;
          if (pendingError !== void 0) {
            error = pendingError;
            pendingError = void 0;
          }
          errors = errors || {};
          if (skipLoaderErrorBubbling) {
            errors[id] = error;
          } else {
            let boundaryMatch = findNearestBoundary(matches, id);
            if (errors[boundaryMatch.route.id] == null) {
              errors[boundaryMatch.route.id] = error;
            }
          }
          if (!isStaticHandler) {
            loaderData[id] = ResetLoaderDataSymbol;
          }
          if (!foundError) {
            foundError = true;
            statusCode = isRouteErrorResponse(result.error) ? result.error.status : 500;
          }
          if (result.headers) {
            loaderHeaders[id] = result.headers;
          }
        } else {
          loaderData[id] = result.data;
          if (result.statusCode && result.statusCode !== 200 && !foundError) {
            statusCode = result.statusCode;
          }
          if (result.headers) {
            loaderHeaders[id] = result.headers;
          }
        }
      });
      if (pendingError !== void 0 && pendingActionResult) {
        errors = { [pendingActionResult[0]]: pendingError };
        loaderData[pendingActionResult[0]] = void 0;
      }
      return {
        loaderData,
        errors,
        statusCode: statusCode || 200,
        loaderHeaders
      };
    }
    function processLoaderData(state, matches, results, pendingActionResult, revalidatingFetchers, fetcherResults) {
      let { loaderData, errors } = processRouteLoaderData(
        matches,
        results,
        pendingActionResult
      );
      revalidatingFetchers.forEach((rf) => {
        let { key, match, controller } = rf;
        let result = fetcherResults[key];
        invariant(result, "Did not find corresponding fetcher result");
        if (controller && controller.signal.aborted) {
          return;
        } else if (isErrorResult(result)) {
          let boundaryMatch = findNearestBoundary(state.matches, match == null ? void 0 : match.route.id);
          if (!(errors && errors[boundaryMatch.route.id])) {
            errors = {
              ...errors,
              [boundaryMatch.route.id]: result.error
            };
          }
          state.fetchers.delete(key);
        } else if (isRedirectResult(result)) {
          invariant(false, "Unhandled fetcher revalidation redirect");
        } else {
          let doneFetcher = getDoneFetcher(result.data);
          state.fetchers.set(key, doneFetcher);
        }
      });
      return { loaderData, errors };
    }
    function mergeLoaderData(loaderData, newLoaderData, matches, errors) {
      let mergedLoaderData = Object.entries(newLoaderData).filter(([, v]) => v !== ResetLoaderDataSymbol).reduce((merged, [k, v]) => {
        merged[k] = v;
        return merged;
      }, {});
      for (let match of matches) {
        let id = match.route.id;
        if (!newLoaderData.hasOwnProperty(id) && loaderData.hasOwnProperty(id) && match.route.loader) {
          mergedLoaderData[id] = loaderData[id];
        }
        if (errors && errors.hasOwnProperty(id)) {
          break;
        }
      }
      return mergedLoaderData;
    }
    function getActionDataForCommit(pendingActionResult) {
      if (!pendingActionResult) {
        return {};
      }
      return isErrorResult(pendingActionResult[1]) ? {
        // Clear out prior actionData on errors
        actionData: {}
      } : {
        actionData: {
          [pendingActionResult[0]]: pendingActionResult[1].data
        }
      };
    }
    function findNearestBoundary(matches, routeId) {
      let eligibleMatches = routeId ? matches.slice(0, matches.findIndex((m) => m.route.id === routeId) + 1) : [...matches];
      return eligibleMatches.reverse().find((m) => m.route.hasErrorBoundary === true) || matches[0];
    }
    function getShortCircuitMatches(routes) {
      let route = routes.length === 1 ? routes[0] : routes.find((r) => r.index || !r.path || r.path === "/") || {
        id: `__shim-error-route__`
      };
      return {
        matches: [
          {
            params: {},
            pathname: "",
            pathnameBase: "",
            route
          }
        ],
        route
      };
    }
    function getInternalRouterError(status, {
      pathname,
      routeId,
      method,
      type,
      message
    } = {}) {
      let statusText = "Unknown Server Error";
      let errorMessage = "Unknown @remix-run/router error";
      if (status === 400) {
        statusText = "Bad Request";
        if (method && pathname && routeId) {
          errorMessage = `You made a ${method} request to "${pathname}" but did not provide a \`loader\` for route "${routeId}", so there is no way to handle the request.`;
        } else if (type === "invalid-body") {
          errorMessage = "Unable to encode submission body";
        }
      } else if (status === 403) {
        statusText = "Forbidden";
        errorMessage = `Route "${routeId}" does not match URL "${pathname}"`;
      } else if (status === 404) {
        statusText = "Not Found";
        errorMessage = `No route matches URL "${pathname}"`;
      } else if (status === 405) {
        statusText = "Method Not Allowed";
        if (method && pathname && routeId) {
          errorMessage = `You made a ${method.toUpperCase()} request to "${pathname}" but did not provide an \`action\` for route "${routeId}", so there is no way to handle the request.`;
        } else if (method) {
          errorMessage = `Invalid request method "${method.toUpperCase()}"`;
        }
      }
      return new ErrorResponseImpl(
        status || 500,
        statusText,
        new Error(errorMessage),
        true
      );
    }
    function findRedirect(results) {
      let entries = Object.entries(results);
      for (let i = entries.length - 1; i >= 0; i--) {
        let [key, result] = entries[i];
        if (isRedirectResult(result)) {
          return { key, result };
        }
      }
    }
    function stripHashFromPath(path) {
      let parsedPath = typeof path === "string" ? parsePath(path) : path;
      return createPath({ ...parsedPath, hash: "" });
    }
    function isHashChangeOnly(a, b) {
      if (a.pathname !== b.pathname || a.search !== b.search) {
        return false;
      }
      if (a.hash === "") {
        return b.hash !== "";
      } else if (a.hash === b.hash) {
        return true;
      } else if (b.hash !== "") {
        return true;
      }
      return false;
    }
    function isRedirectDataStrategyResult(result) {
      return isResponse(result.result) && redirectStatusCodes.has(result.result.status);
    }
    function isErrorResult(result) {
      return result.type === "error";
    }
    function isRedirectResult(result) {
      return (result && result.type) === "redirect";
    }
    function isDataWithResponseInit(value) {
      return typeof value === "object" && value != null && "type" in value && "data" in value && "init" in value && value.type === "DataWithResponseInit";
    }
    function isResponse(value) {
      return value != null && typeof value.status === "number" && typeof value.statusText === "string" && typeof value.headers === "object" && typeof value.body !== "undefined";
    }
    function isValidMethod(method) {
      return validRequestMethods.has(method.toUpperCase());
    }
    function isMutationMethod(method) {
      return validMutationMethods.has(method.toUpperCase());
    }
    function hasNakedIndexQuery(search) {
      return new URLSearchParams(search).getAll("index").some((v) => v === "");
    }
    function getTargetMatch(matches, location) {
      let search = typeof location === "string" ? parsePath(location).search : location.search;
      if (matches[matches.length - 1].route.index && hasNakedIndexQuery(search || "")) {
        return matches[matches.length - 1];
      }
      let pathMatches = getPathContributingMatches(matches);
      return pathMatches[pathMatches.length - 1];
    }
    function getSubmissionFromNavigation(navigation) {
      let { formMethod, formAction, formEncType, text, formData, json } = navigation;
      if (!formMethod || !formAction || !formEncType) {
        return;
      }
      if (text != null) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData: void 0,
          json: void 0,
          text
        };
      } else if (formData != null) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData,
          json: void 0,
          text: void 0
        };
      } else if (json !== void 0) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData: void 0,
          json,
          text: void 0
        };
      }
    }
    function getLoadingNavigation(location, submission) {
      if (submission) {
        let navigation = {
          state: "loading",
          location,
          formMethod: submission.formMethod,
          formAction: submission.formAction,
          formEncType: submission.formEncType,
          formData: submission.formData,
          json: submission.json,
          text: submission.text
        };
        return navigation;
      } else {
        let navigation = {
          state: "loading",
          location,
          formMethod: void 0,
          formAction: void 0,
          formEncType: void 0,
          formData: void 0,
          json: void 0,
          text: void 0
        };
        return navigation;
      }
    }
    function getSubmittingNavigation(location, submission) {
      let navigation = {
        state: "submitting",
        location,
        formMethod: submission.formMethod,
        formAction: submission.formAction,
        formEncType: submission.formEncType,
        formData: submission.formData,
        json: submission.json,
        text: submission.text
      };
      return navigation;
    }
    function getLoadingFetcher(submission, data2) {
      if (submission) {
        let fetcher = {
          state: "loading",
          formMethod: submission.formMethod,
          formAction: submission.formAction,
          formEncType: submission.formEncType,
          formData: submission.formData,
          json: submission.json,
          text: submission.text,
          data: data2
        };
        return fetcher;
      } else {
        let fetcher = {
          state: "loading",
          formMethod: void 0,
          formAction: void 0,
          formEncType: void 0,
          formData: void 0,
          json: void 0,
          text: void 0,
          data: data2
        };
        return fetcher;
      }
    }
    function getSubmittingFetcher(submission, existingFetcher) {
      let fetcher = {
        state: "submitting",
        formMethod: submission.formMethod,
        formAction: submission.formAction,
        formEncType: submission.formEncType,
        formData: submission.formData,
        json: submission.json,
        text: submission.text,
        data: existingFetcher ? existingFetcher.data : void 0
      };
      return fetcher;
    }
    function getDoneFetcher(data2) {
      let fetcher = {
        state: "idle",
        formMethod: void 0,
        formAction: void 0,
        formEncType: void 0,
        formData: void 0,
        json: void 0,
        text: void 0,
        data: data2
      };
      return fetcher;
    }
    function restoreAppliedTransitions(_window, transitions) {
      try {
        let sessionPositions = _window.sessionStorage.getItem(
          TRANSITIONS_STORAGE_KEY
        );
        if (sessionPositions) {
          let json = JSON.parse(sessionPositions);
          for (let [k, v] of Object.entries(json || {})) {
            if (v && Array.isArray(v)) {
              transitions.set(k, new Set(v || []));
            }
          }
        }
      } catch (e) {
      }
    }
    function persistAppliedTransitions(_window, transitions) {
      if (transitions.size > 0) {
        let json = {};
        for (let [k, v] of transitions) {
          json[k] = [...v];
        }
        try {
          _window.sessionStorage.setItem(
            TRANSITIONS_STORAGE_KEY,
            JSON.stringify(json)
          );
        } catch (error) {
          warning(
            false,
            `Failed to save applied view transitions in sessionStorage (${error}).`
          );
        }
      }
    }
    function createDeferred() {
      let resolve;
      let reject;
      let promise = new Promise((res, rej) => {
        resolve = async (val) => {
          res(val);
          try {
            await promise;
          } catch (e) {
          }
        };
        reject = async (error) => {
          rej(error);
          try {
            await promise;
          } catch (e) {
          }
        };
      });
      return {
        promise,
        //@ts-ignore
        resolve,
        //@ts-ignore
        reject
      };
    }
    var React3 = __toESM2(require_react());
    var React2 = __toESM2(require_react());
    var DataRouterContext = React2.createContext(null);
    DataRouterContext.displayName = "DataRouter";
    var DataRouterStateContext = React2.createContext(null);
    DataRouterStateContext.displayName = "DataRouterState";
    var ViewTransitionContext = React2.createContext({
      isTransitioning: false
    });
    ViewTransitionContext.displayName = "ViewTransition";
    var FetchersContext = React2.createContext(
      /* @__PURE__ */ new Map()
    );
    FetchersContext.displayName = "Fetchers";
    var AwaitContext = React2.createContext(null);
    AwaitContext.displayName = "Await";
    var NavigationContext = React2.createContext(
      null
    );
    NavigationContext.displayName = "Navigation";
    var LocationContext = React2.createContext(
      null
    );
    LocationContext.displayName = "Location";
    var RouteContext = React2.createContext({
      outlet: null,
      matches: [],
      isDataRoute: false
    });
    RouteContext.displayName = "Route";
    var RouteErrorContext = React2.createContext(null);
    RouteErrorContext.displayName = "RouteError";
    var React22 = __toESM2(require_react());
    var ENABLE_DEV_WARNINGS = true;
    function useInRouterContext() {
      return React22.useContext(LocationContext) != null;
    }
    function useLocation() {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useLocation() may be used only in the context of a <Router> component.`
      );
      return React22.useContext(LocationContext).location;
    }
    var OutletContext = React22.createContext(null);
    function useRoutesImpl(routes, locationArg, dataRouterState, future) {
      var _a;
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useRoutes() may be used only in the context of a <Router> component.`
      );
      let { navigator: navigator2, static: isStatic } = React22.useContext(NavigationContext);
      let { matches: parentMatches } = React22.useContext(RouteContext);
      let routeMatch = parentMatches[parentMatches.length - 1];
      let parentParams = routeMatch ? routeMatch.params : {};
      let parentPathname = routeMatch ? routeMatch.pathname : "/";
      let parentPathnameBase = routeMatch ? routeMatch.pathnameBase : "/";
      let parentRoute = routeMatch && routeMatch.route;
      if (ENABLE_DEV_WARNINGS) {
        let parentPath = parentRoute && parentRoute.path || "";
        warningOnce(
          parentPathname,
          !parentRoute || parentPath.endsWith("*") || parentPath.endsWith("*?"),
          `You rendered descendant <Routes> (or called \`useRoutes()\`) at "${parentPathname}" (under <Route path="${parentPath}">) but the parent route path has no trailing "*". This means if you navigate deeper, the parent won't match anymore and therefore the child routes will never render.

Please change the parent <Route path="${parentPath}"> to <Route path="${parentPath === "/" ? "*" : `${parentPath}/*`}">.`
        );
      }
      let locationFromContext = useLocation();
      let location;
      if (locationArg) {
        let parsedLocationArg = typeof locationArg === "string" ? parsePath(locationArg) : locationArg;
        invariant(
          parentPathnameBase === "/" || ((_a = parsedLocationArg.pathname) == null ? void 0 : _a.startsWith(parentPathnameBase)),
          `When overriding the location using \`<Routes location>\` or \`useRoutes(routes, location)\`, the location pathname must begin with the portion of the URL pathname that was matched by all parent routes. The current pathname base is "${parentPathnameBase}" but pathname "${parsedLocationArg.pathname}" was given in the \`location\` prop.`
        );
        location = parsedLocationArg;
      } else {
        location = locationFromContext;
      }
      let pathname = location.pathname || "/";
      let remainingPathname = pathname;
      if (parentPathnameBase !== "/") {
        let parentSegments = parentPathnameBase.replace(/^\//, "").split("/");
        let segments = pathname.replace(/^\//, "").split("/");
        remainingPathname = "/" + segments.slice(parentSegments.length).join("/");
      }
      let matches = !isStatic && dataRouterState && dataRouterState.matches && dataRouterState.matches.length > 0 ? dataRouterState.matches : matchRoutes(routes, { pathname: remainingPathname });
      if (ENABLE_DEV_WARNINGS) {
        warning(
          parentRoute || matches != null,
          `No routes matched location "${location.pathname}${location.search}${location.hash}" `
        );
        warning(
          matches == null || matches[matches.length - 1].route.element !== void 0 || matches[matches.length - 1].route.Component !== void 0 || matches[matches.length - 1].route.lazy !== void 0,
          `Matched leaf route at location "${location.pathname}${location.search}${location.hash}" does not have an element or Component. This means it will render an <Outlet /> with a null value by default resulting in an "empty" page.`
        );
      }
      let renderedMatches = _renderMatches(
        matches && matches.map(
          (match) => Object.assign({}, match, {
            params: Object.assign({}, parentParams, match.params),
            pathname: joinPaths([
              parentPathnameBase,
              // Re-encode pathnames that were decoded inside matchRoutes
              navigator2.encodeLocation ? navigator2.encodeLocation(match.pathname).pathname : match.pathname
            ]),
            pathnameBase: match.pathnameBase === "/" ? parentPathnameBase : joinPaths([
              parentPathnameBase,
              // Re-encode pathnames that were decoded inside matchRoutes
              navigator2.encodeLocation ? navigator2.encodeLocation(match.pathnameBase).pathname : match.pathnameBase
            ])
          })
        ),
        parentMatches,
        dataRouterState,
        future
      );
      if (locationArg && renderedMatches) {
        return React22.createElement(
          LocationContext.Provider,
          {
            value: {
              location: {
                pathname: "/",
                search: "",
                hash: "",
                state: null,
                key: "default",
                ...location
              },
              navigationType: "POP"
              /* Pop */
            }
          },
          renderedMatches
        );
      }
      return renderedMatches;
    }
    function DefaultErrorComponent() {
      let error = useRouteError();
      let message = isRouteErrorResponse(error) ? `${error.status} ${error.statusText}` : error instanceof Error ? error.message : JSON.stringify(error);
      let stack = error instanceof Error ? error.stack : null;
      let lightgrey = "rgba(200,200,200, 0.5)";
      let preStyles = { padding: "0.5rem", backgroundColor: lightgrey };
      let codeStyles = { padding: "2px 4px", backgroundColor: lightgrey };
      let devInfo = null;
      if (ENABLE_DEV_WARNINGS) {
        console.error(
          "Error handled by React Router default ErrorBoundary:",
          error
        );
        devInfo = React22.createElement(React22.Fragment, null, React22.createElement("p", null, "💿 Hey developer 👋"), React22.createElement("p", null, "You can provide a way better UX than this when your app throws errors by providing your own ", React22.createElement("code", { style: codeStyles }, "ErrorBoundary"), " or", " ", React22.createElement("code", { style: codeStyles }, "errorElement"), " prop on your route."));
      }
      return React22.createElement(React22.Fragment, null, React22.createElement("h2", null, "Unexpected Application Error!"), React22.createElement("h3", { style: { fontStyle: "italic" } }, message), stack ? React22.createElement("pre", { style: preStyles }, stack) : null, devInfo);
    }
    var defaultErrorElement = React22.createElement(DefaultErrorComponent, null);
    var RenderErrorBoundary = class extends React22.Component {
      constructor(props) {
        super(props);
        this.state = {
          location: props.location,
          revalidation: props.revalidation,
          error: props.error
        };
      }
      static getDerivedStateFromError(error) {
        return { error };
      }
      static getDerivedStateFromProps(props, state) {
        if (state.location !== props.location || state.revalidation !== "idle" && props.revalidation === "idle") {
          return {
            error: props.error,
            location: props.location,
            revalidation: props.revalidation
          };
        }
        return {
          error: props.error !== void 0 ? props.error : state.error,
          location: state.location,
          revalidation: props.revalidation || state.revalidation
        };
      }
      componentDidCatch(error, errorInfo) {
        console.error(
          "React Router caught the following error during render",
          error,
          errorInfo
        );
      }
      render() {
        return this.state.error !== void 0 ? React22.createElement(RouteContext.Provider, { value: this.props.routeContext }, React22.createElement(
          RouteErrorContext.Provider,
          {
            value: this.state.error,
            children: this.props.component
          }
        )) : this.props.children;
      }
    };
    function RenderedRoute({ routeContext, match, children }) {
      let dataRouterContext = React22.useContext(DataRouterContext);
      if (dataRouterContext && dataRouterContext.static && dataRouterContext.staticContext && (match.route.errorElement || match.route.ErrorBoundary)) {
        dataRouterContext.staticContext._deepestRenderedBoundaryId = match.route.id;
      }
      return React22.createElement(RouteContext.Provider, { value: routeContext }, children);
    }
    function _renderMatches(matches, parentMatches = [], dataRouterState = null, future = null) {
      if (matches == null) {
        if (!dataRouterState) {
          return null;
        }
        if (dataRouterState.errors) {
          matches = dataRouterState.matches;
        } else if (parentMatches.length === 0 && !dataRouterState.initialized && dataRouterState.matches.length > 0) {
          matches = dataRouterState.matches;
        } else {
          return null;
        }
      }
      let renderedMatches = matches;
      let errors = dataRouterState == null ? void 0 : dataRouterState.errors;
      if (errors != null) {
        let errorIndex = renderedMatches.findIndex(
          (m) => m.route.id && (errors == null ? void 0 : errors[m.route.id]) !== void 0
        );
        invariant(
          errorIndex >= 0,
          `Could not find a matching route for errors on route IDs: ${Object.keys(
            errors
          ).join(",")}`
        );
        renderedMatches = renderedMatches.slice(
          0,
          Math.min(renderedMatches.length, errorIndex + 1)
        );
      }
      let renderFallback = false;
      let fallbackIndex = -1;
      if (dataRouterState) {
        for (let i = 0; i < renderedMatches.length; i++) {
          let match = renderedMatches[i];
          if (match.route.HydrateFallback || match.route.hydrateFallbackElement) {
            fallbackIndex = i;
          }
          if (match.route.id) {
            let { loaderData, errors: errors2 } = dataRouterState;
            let needsToRunLoader = match.route.loader && !loaderData.hasOwnProperty(match.route.id) && (!errors2 || errors2[match.route.id] === void 0);
            if (match.route.lazy || needsToRunLoader) {
              renderFallback = true;
              if (fallbackIndex >= 0) {
                renderedMatches = renderedMatches.slice(0, fallbackIndex + 1);
              } else {
                renderedMatches = [renderedMatches[0]];
              }
              break;
            }
          }
        }
      }
      return renderedMatches.reduceRight((outlet, match, index) => {
        let error;
        let shouldRenderHydrateFallback = false;
        let errorElement = null;
        let hydrateFallbackElement = null;
        if (dataRouterState) {
          error = errors && match.route.id ? errors[match.route.id] : void 0;
          errorElement = match.route.errorElement || defaultErrorElement;
          if (renderFallback) {
            if (fallbackIndex < 0 && index === 0) {
              warningOnce(
                "route-fallback",
                false,
                "No `HydrateFallback` element provided to render during initial hydration"
              );
              shouldRenderHydrateFallback = true;
              hydrateFallbackElement = null;
            } else if (fallbackIndex === index) {
              shouldRenderHydrateFallback = true;
              hydrateFallbackElement = match.route.hydrateFallbackElement || null;
            }
          }
        }
        let matches2 = parentMatches.concat(renderedMatches.slice(0, index + 1));
        let getChildren = () => {
          let children;
          if (error) {
            children = errorElement;
          } else if (shouldRenderHydrateFallback) {
            children = hydrateFallbackElement;
          } else if (match.route.Component) {
            children = React22.createElement(match.route.Component, null);
          } else if (match.route.element) {
            children = match.route.element;
          } else {
            children = outlet;
          }
          return React22.createElement(
            RenderedRoute,
            {
              match,
              routeContext: {
                outlet,
                matches: matches2,
                isDataRoute: dataRouterState != null
              },
              children
            }
          );
        };
        return dataRouterState && (match.route.ErrorBoundary || match.route.errorElement || index === 0) ? React22.createElement(
          RenderErrorBoundary,
          {
            location: dataRouterState.location,
            revalidation: dataRouterState.revalidation,
            component: errorElement,
            error,
            children: getChildren(),
            routeContext: { outlet: null, matches: matches2, isDataRoute: true }
          }
        ) : getChildren();
      }, null);
    }
    function getDataRouterConsoleError(hookName) {
      return `${hookName} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`;
    }
    function useDataRouterState(hookName) {
      let state = React22.useContext(DataRouterStateContext);
      invariant(state, getDataRouterConsoleError(hookName));
      return state;
    }
    function useRouteContext(hookName) {
      let route = React22.useContext(RouteContext);
      invariant(route, getDataRouterConsoleError(hookName));
      return route;
    }
    function useCurrentRouteId(hookName) {
      let route = useRouteContext(hookName);
      let thisRoute = route.matches[route.matches.length - 1];
      invariant(
        thisRoute.route.id,
        `${hookName} can only be used on routes that contain a unique "id"`
      );
      return thisRoute.route.id;
    }
    function useRouteError() {
      var _a;
      let error = React22.useContext(RouteErrorContext);
      let state = useDataRouterState(
        "useRouteError"
        /* UseRouteError */
      );
      let routeId = useCurrentRouteId(
        "useRouteError"
        /* UseRouteError */
      );
      if (error !== void 0) {
        return error;
      }
      return (_a = state.errors) == null ? void 0 : _a[routeId];
    }
    var alreadyWarned = {};
    function warningOnce(key, cond, message) {
      if (!cond && !alreadyWarned[key]) {
        alreadyWarned[key] = true;
        warning(false, message);
      }
    }
    var alreadyWarned2 = {};
    function warnOnce(condition, message) {
      if (!condition && !alreadyWarned2[message]) {
        alreadyWarned2[message] = true;
        console.warn(message);
      }
    }
    var ENABLE_DEV_WARNINGS2 = true;
    function mapRouteProperties(route) {
      let updates = {
        // Note: this check also occurs in createRoutesFromChildren so update
        // there if you change this -- please and thank you!
        hasErrorBoundary: route.hasErrorBoundary || route.ErrorBoundary != null || route.errorElement != null
      };
      if (route.Component) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.element) {
            warning(
              false,
              "You should not include both `Component` and `element` on your route - `Component` will be used."
            );
          }
        }
        Object.assign(updates, {
          element: React3.createElement(route.Component),
          Component: void 0
        });
      }
      if (route.HydrateFallback) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.hydrateFallbackElement) {
            warning(
              false,
              "You should not include both `HydrateFallback` and `hydrateFallbackElement` on your route - `HydrateFallback` will be used."
            );
          }
        }
        Object.assign(updates, {
          hydrateFallbackElement: React3.createElement(route.HydrateFallback),
          HydrateFallback: void 0
        });
      }
      if (route.ErrorBoundary) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.errorElement) {
            warning(
              false,
              "You should not include both `ErrorBoundary` and `errorElement` on your route - `ErrorBoundary` will be used."
            );
          }
        }
        Object.assign(updates, {
          errorElement: React3.createElement(route.ErrorBoundary),
          ErrorBoundary: void 0
        });
      }
      return updates;
    }
    var Deferred = class {
      constructor() {
        this.status = "pending";
        this.promise = new Promise((resolve, reject) => {
          this.resolve = (value) => {
            if (this.status === "pending") {
              this.status = "resolved";
              resolve(value);
            }
          };
          this.reject = (reason) => {
            if (this.status === "pending") {
              this.status = "rejected";
              reject(reason);
            }
          };
        });
      }
    };
    function RouterProvider2({
      router: router2,
      flushSync: reactDomFlushSyncImpl
    }) {
      let [state, setStateImpl] = React3.useState(router2.state);
      let [pendingState, setPendingState] = React3.useState();
      let [vtContext, setVtContext] = React3.useState({
        isTransitioning: false
      });
      let [renderDfd, setRenderDfd] = React3.useState();
      let [transition, setTransition] = React3.useState();
      let [interruption, setInterruption] = React3.useState();
      let fetcherData = React3.useRef(/* @__PURE__ */ new Map());
      let setState = React3.useCallback(
        (newState, { deletedFetchers, flushSync: flushSync2, viewTransitionOpts }) => {
          newState.fetchers.forEach((fetcher, key) => {
            if (fetcher.data !== void 0) {
              fetcherData.current.set(key, fetcher.data);
            }
          });
          deletedFetchers.forEach((key) => fetcherData.current.delete(key));
          warnOnce(
            flushSync2 === false || reactDomFlushSyncImpl != null,
            'You provided the `flushSync` option to a router update, but you are not using the `<RouterProvider>` from `react-router/dom` so `ReactDOM.flushSync()` is unavailable.  Please update your app to `import { RouterProvider } from "react-router/dom"` and ensure you have `react-dom` installed as a dependency to use the `flushSync` option.'
          );
          let isViewTransitionAvailable = router2.window != null && router2.window.document != null && typeof router2.window.document.startViewTransition === "function";
          warnOnce(
            viewTransitionOpts == null || isViewTransitionAvailable,
            "You provided the `viewTransition` option to a router update, but you do not appear to be running in a DOM environment as `window.startViewTransition` is not available."
          );
          if (!viewTransitionOpts || !isViewTransitionAvailable) {
            if (reactDomFlushSyncImpl && flushSync2) {
              reactDomFlushSyncImpl(() => setStateImpl(newState));
            } else {
              React3.startTransition(() => setStateImpl(newState));
            }
            return;
          }
          if (reactDomFlushSyncImpl && flushSync2) {
            reactDomFlushSyncImpl(() => {
              if (transition) {
                renderDfd && renderDfd.resolve();
                transition.skipTransition();
              }
              setVtContext({
                isTransitioning: true,
                flushSync: true,
                currentLocation: viewTransitionOpts.currentLocation,
                nextLocation: viewTransitionOpts.nextLocation
              });
            });
            let t = router2.window.document.startViewTransition(() => {
              reactDomFlushSyncImpl(() => setStateImpl(newState));
            });
            t.finished.finally(() => {
              reactDomFlushSyncImpl(() => {
                setRenderDfd(void 0);
                setTransition(void 0);
                setPendingState(void 0);
                setVtContext({ isTransitioning: false });
              });
            });
            reactDomFlushSyncImpl(() => setTransition(t));
            return;
          }
          if (transition) {
            renderDfd && renderDfd.resolve();
            transition.skipTransition();
            setInterruption({
              state: newState,
              currentLocation: viewTransitionOpts.currentLocation,
              nextLocation: viewTransitionOpts.nextLocation
            });
          } else {
            setPendingState(newState);
            setVtContext({
              isTransitioning: true,
              flushSync: false,
              currentLocation: viewTransitionOpts.currentLocation,
              nextLocation: viewTransitionOpts.nextLocation
            });
          }
        },
        [router2.window, reactDomFlushSyncImpl, transition, renderDfd]
      );
      React3.useLayoutEffect(() => router2.subscribe(setState), [router2, setState]);
      React3.useEffect(() => {
        if (vtContext.isTransitioning && !vtContext.flushSync) {
          setRenderDfd(new Deferred());
        }
      }, [vtContext]);
      React3.useEffect(() => {
        if (renderDfd && pendingState && router2.window) {
          let newState = pendingState;
          let renderPromise = renderDfd.promise;
          let transition2 = router2.window.document.startViewTransition(async () => {
            React3.startTransition(() => setStateImpl(newState));
            await renderPromise;
          });
          transition2.finished.finally(() => {
            setRenderDfd(void 0);
            setTransition(void 0);
            setPendingState(void 0);
            setVtContext({ isTransitioning: false });
          });
          setTransition(transition2);
        }
      }, [pendingState, renderDfd, router2.window]);
      React3.useEffect(() => {
        if (renderDfd && pendingState && state.location.key === pendingState.location.key) {
          renderDfd.resolve();
        }
      }, [renderDfd, transition, state.location, pendingState]);
      React3.useEffect(() => {
        if (!vtContext.isTransitioning && interruption) {
          setPendingState(interruption.state);
          setVtContext({
            isTransitioning: true,
            flushSync: false,
            currentLocation: interruption.currentLocation,
            nextLocation: interruption.nextLocation
          });
          setInterruption(void 0);
        }
      }, [vtContext.isTransitioning, interruption]);
      let navigator2 = React3.useMemo(() => {
        return {
          createHref: router2.createHref,
          encodeLocation: router2.encodeLocation,
          go: (n) => router2.navigate(n),
          push: (to, state2, opts) => router2.navigate(to, {
            state: state2,
            preventScrollReset: opts == null ? void 0 : opts.preventScrollReset
          }),
          replace: (to, state2, opts) => router2.navigate(to, {
            replace: true,
            state: state2,
            preventScrollReset: opts == null ? void 0 : opts.preventScrollReset
          })
        };
      }, [router2]);
      let basename = router2.basename || "/";
      let dataRouterContext = React3.useMemo(
        () => ({
          router: router2,
          navigator: navigator2,
          static: false,
          basename
        }),
        [router2, navigator2, basename]
      );
      return React3.createElement(React3.Fragment, null, React3.createElement(DataRouterContext.Provider, { value: dataRouterContext }, React3.createElement(DataRouterStateContext.Provider, { value: state }, React3.createElement(FetchersContext.Provider, { value: fetcherData.current }, React3.createElement(ViewTransitionContext.Provider, { value: vtContext }, React3.createElement(
        Router,
        {
          basename,
          location: state.location,
          navigationType: state.historyAction,
          navigator: navigator2
        },
        React3.createElement(
          MemoizedDataRoutes,
          {
            routes: router2.routes,
            future: router2.future,
            state
          }
        )
      ))))), null);
    }
    var MemoizedDataRoutes = React3.memo(DataRoutes);
    function DataRoutes({
      routes,
      future,
      state
    }) {
      return useRoutesImpl(routes, void 0, state, future);
    }
    function Router({
      basename: basenameProp = "/",
      children = null,
      location: locationProp,
      navigationType = "POP",
      navigator: navigator2,
      static: staticProp = false
    }) {
      invariant(
        !useInRouterContext(),
        `You cannot render a <Router> inside another <Router>. You should never have more than one in your app.`
      );
      let basename = basenameProp.replace(/^\/*/, "/");
      let navigationContext = React3.useMemo(
        () => ({
          basename,
          navigator: navigator2,
          static: staticProp,
          future: {}
        }),
        [basename, navigator2, staticProp]
      );
      if (typeof locationProp === "string") {
        locationProp = parsePath(locationProp);
      }
      let {
        pathname = "/",
        search = "",
        hash = "",
        state = null,
        key = "default"
      } = locationProp;
      let locationContext = React3.useMemo(() => {
        let trailingPathname = stripBasename(pathname, basename);
        if (trailingPathname == null) {
          return null;
        }
        return {
          location: {
            pathname: trailingPathname,
            search,
            hash,
            state,
            key
          },
          navigationType
        };
      }, [basename, pathname, search, hash, state, key, navigationType]);
      warning(
        locationContext != null,
        `<Router basename="${basename}"> is not able to match the URL "${pathname}${search}${hash}" because it does not start with the basename, so the <Router> won't render anything.`
      );
      if (locationContext == null) {
        return null;
      }
      return React3.createElement(NavigationContext.Provider, { value: navigationContext }, React3.createElement(LocationContext.Provider, { children, value: locationContext }));
    }
    var React9 = __toESM2(require_react());
    function invariant2(value, message) {
      if (value === false || value === null || typeof value === "undefined") {
        throw new Error(message);
      }
    }
    async function loadRouteModule(route, routeModulesCache) {
      if (route.id in routeModulesCache) {
        return routeModulesCache[route.id];
      }
      try {
        let routeModule = await import(
          /* @vite-ignore */
          /* webpackIgnore: true */
          route.module
        );
        routeModulesCache[route.id] = routeModule;
        return routeModule;
      } catch (error) {
        console.error(
          `Error loading route module \`${route.module}\`, reloading page...`
        );
        console.error(error);
        if (window.__reactRouterContext && window.__reactRouterContext.isSpaMode && // @ts-expect-error
        void 0) {
          throw error;
        }
        window.location.reload();
        return new Promise(() => {
        });
      }
    }
    function getRouteCssDescriptors(route) {
      if (!route.css) return [];
      return route.css.map((href) => ({ rel: "stylesheet", href }));
    }
    async function prefetchRouteCss(route) {
      if (!route.css) return;
      let descriptors = getRouteCssDescriptors(route);
      await Promise.all(descriptors.map(prefetchStyleLink));
    }
    async function prefetchStyleLinks(route, routeModule) {
      if (!route.css && !routeModule.links || !isPreloadSupported()) return;
      let descriptors = [];
      if (route.css) {
        descriptors.push(...getRouteCssDescriptors(route));
      }
      if (routeModule.links) {
        descriptors.push(...routeModule.links());
      }
      if (descriptors.length === 0) return;
      let styleLinks = [];
      for (let descriptor of descriptors) {
        if (!isPageLinkDescriptor(descriptor) && descriptor.rel === "stylesheet") {
          styleLinks.push({
            ...descriptor,
            rel: "preload",
            as: "style"
          });
        }
      }
      await Promise.all(styleLinks.map(prefetchStyleLink));
    }
    async function prefetchStyleLink(descriptor) {
      return new Promise((resolve) => {
        if (descriptor.media && !window.matchMedia(descriptor.media).matches || document.querySelector(
          `link[rel="stylesheet"][href="${descriptor.href}"]`
        )) {
          return resolve();
        }
        let link = document.createElement("link");
        Object.assign(link, descriptor);
        function removeLink() {
          if (document.head.contains(link)) {
            document.head.removeChild(link);
          }
        }
        link.onload = () => {
          removeLink();
          resolve();
        };
        link.onerror = () => {
          removeLink();
          resolve();
        };
        document.head.appendChild(link);
      });
    }
    function isPageLinkDescriptor(object) {
      return object != null && typeof object.page === "string";
    }
    function getModuleLinkHrefs(matches, manifest, { includeHydrateFallback } = {}) {
      return dedupeHrefs(
        matches.map((match) => {
          let route = manifest.routes[match.route.id];
          if (!route) return [];
          let hrefs = [route.module];
          if (route.clientActionModule) {
            hrefs = hrefs.concat(route.clientActionModule);
          }
          if (route.clientLoaderModule) {
            hrefs = hrefs.concat(route.clientLoaderModule);
          }
          if (includeHydrateFallback && route.hydrateFallbackModule) {
            hrefs = hrefs.concat(route.hydrateFallbackModule);
          }
          if (route.imports) {
            hrefs = hrefs.concat(route.imports);
          }
          return hrefs;
        }).flat(1)
      );
    }
    function dedupeHrefs(hrefs) {
      return [...new Set(hrefs)];
    }
    var _isPreloadSupported;
    function isPreloadSupported() {
      if (_isPreloadSupported !== void 0) {
        return _isPreloadSupported;
      }
      let el = document.createElement("link");
      _isPreloadSupported = el.relList.supports("preload");
      el = null;
      return _isPreloadSupported;
    }
    function createHtml(html) {
      return { __html: html };
    }
    var React4 = __toESM2(require_react());
    var import_turbo_stream = require_turbo_stream();
    async function createRequestInit(request) {
      let init = { signal: request.signal };
      if (request.method !== "GET") {
        init.method = request.method;
        let contentType = request.headers.get("Content-Type");
        if (contentType && /\bapplication\/json\b/.test(contentType)) {
          init.headers = { "Content-Type": contentType };
          init.body = JSON.stringify(await request.json());
        } else if (contentType && /\btext\/plain\b/.test(contentType)) {
          init.headers = { "Content-Type": contentType };
          init.body = await request.text();
        } else if (contentType && /\bapplication\/x-www-form-urlencoded\b/.test(contentType)) {
          init.body = new URLSearchParams(await request.text());
        } else {
          init.body = await request.formData();
        }
      }
      return init;
    }
    var SingleFetchRedirectSymbol = Symbol("SingleFetchRedirect");
    function getSingleFetchDataStrategy(manifest, routeModules, ssr, getRouter) {
      return async ({ request, matches, fetcherKey }) => {
        if (request.method !== "GET") {
          return singleFetchActionStrategy(request, matches);
        }
        if (!ssr) {
          let foundRevalidatingServerLoader = matches.some(
            (m) => {
              var _a, _b;
              return m.shouldLoad && ((_a = manifest.routes[m.route.id]) == null ? void 0 : _a.hasLoader) && !((_b = manifest.routes[m.route.id]) == null ? void 0 : _b.hasClientLoader);
            }
          );
          if (!foundRevalidatingServerLoader) {
            let matchesToLoad = matches.filter((m) => m.shouldLoad);
            let url = stripIndexParam(singleFetchUrl(request.url));
            let init = await createRequestInit(request);
            let results = {};
            await Promise.all(
              matchesToLoad.map(
                (m) => m.resolve(async (handler) => {
                  var _a;
                  try {
                    let result = ((_a = manifest.routes[m.route.id]) == null ? void 0 : _a.hasClientLoader) ? await fetchSingleLoader(handler, url, init, m.route.id) : await handler();
                    results[m.route.id] = { type: "data", result };
                  } catch (e) {
                    results[m.route.id] = { type: "error", result: e };
                  }
                })
              )
            );
            return results;
          }
        }
        if (fetcherKey) {
          return singleFetchLoaderFetcherStrategy(request, matches);
        }
        return singleFetchLoaderNavigationStrategy(
          manifest,
          routeModules,
          ssr,
          getRouter(),
          request,
          matches
        );
      };
    }
    async function singleFetchActionStrategy(request, matches) {
      let actionMatch = matches.find((m) => m.shouldLoad);
      invariant2(actionMatch, "No action match found");
      let actionStatus = void 0;
      let result = await actionMatch.resolve(async (handler) => {
        let result2 = await handler(async () => {
          let url = singleFetchUrl(request.url);
          let init = await createRequestInit(request);
          let { data: data2, status } = await fetchAndDecode(url, init);
          actionStatus = status;
          return unwrapSingleFetchResult(
            data2,
            actionMatch.route.id
          );
        });
        return result2;
      });
      if (isResponse(result.result) || isRouteErrorResponse(result.result)) {
        return { [actionMatch.route.id]: result };
      }
      return {
        [actionMatch.route.id]: {
          type: result.type,
          result: data(result.result, actionStatus)
        }
      };
    }
    async function singleFetchLoaderNavigationStrategy(manifest, routeModules, ssr, router2, request, matches) {
      let routesParams = /* @__PURE__ */ new Set();
      let foundOptOutRoute = false;
      let routeDfds = matches.map(() => createDeferred2());
      let routesLoadedPromise = Promise.all(routeDfds.map((d) => d.promise));
      let singleFetchDfd = createDeferred2();
      let url = stripIndexParam(singleFetchUrl(request.url));
      let init = await createRequestInit(request);
      let results = {};
      let resolvePromise = Promise.all(
        matches.map(
          async (m, i) => m.resolve(async (handler) => {
            var _a;
            routeDfds[i].resolve();
            let manifestRoute = manifest.routes[m.route.id];
            if (!m.shouldLoad) {
              if (!router2.state.initialized) {
                return;
              }
              if (m.route.id in router2.state.loaderData && manifestRoute && manifestRoute.hasLoader && ((_a = routeModules[m.route.id]) == null ? void 0 : _a.shouldRevalidate)) {
                foundOptOutRoute = true;
                return;
              }
            }
            if (manifestRoute && manifestRoute.hasClientLoader) {
              if (manifestRoute.hasLoader) {
                foundOptOutRoute = true;
              }
              try {
                let result = await fetchSingleLoader(
                  handler,
                  url,
                  init,
                  m.route.id
                );
                results[m.route.id] = { type: "data", result };
              } catch (e) {
                results[m.route.id] = { type: "error", result: e };
              }
              return;
            }
            if (manifestRoute && manifestRoute.hasLoader) {
              routesParams.add(m.route.id);
            }
            try {
              let result = await handler(async () => {
                let data2 = await singleFetchDfd.promise;
                return unwrapSingleFetchResults(data2, m.route.id);
              });
              results[m.route.id] = {
                type: "data",
                result
              };
            } catch (e) {
              results[m.route.id] = {
                type: "error",
                result: e
              };
            }
          })
        )
      );
      await routesLoadedPromise;
      if ((!router2.state.initialized || routesParams.size === 0) && !window.__reactRouterHdrActive) {
        singleFetchDfd.resolve({});
      } else {
        try {
          if (ssr && foundOptOutRoute && routesParams.size > 0) {
            url.searchParams.set(
              "_routes",
              matches.filter((m) => routesParams.has(m.route.id)).map((m) => m.route.id).join(",")
            );
          }
          let data2 = await fetchAndDecode(url, init);
          singleFetchDfd.resolve(data2.data);
        } catch (e) {
          singleFetchDfd.reject(e);
        }
      }
      await resolvePromise;
      return results;
    }
    async function singleFetchLoaderFetcherStrategy(request, matches) {
      let fetcherMatch = matches.find((m) => m.shouldLoad);
      invariant2(fetcherMatch, "No fetcher match found");
      let result = await fetcherMatch.resolve(async (handler) => {
        let url = stripIndexParam(singleFetchUrl(request.url));
        let init = await createRequestInit(request);
        return fetchSingleLoader(handler, url, init, fetcherMatch.route.id);
      });
      return { [fetcherMatch.route.id]: result };
    }
    function fetchSingleLoader(handler, url, init, routeId) {
      return handler(async () => {
        let singleLoaderUrl = new URL(url);
        singleLoaderUrl.searchParams.set("_routes", routeId);
        let { data: data2 } = await fetchAndDecode(singleLoaderUrl, init);
        return unwrapSingleFetchResults(data2, routeId);
      });
    }
    function stripIndexParam(url) {
      let indexValues = url.searchParams.getAll("index");
      url.searchParams.delete("index");
      let indexValuesToKeep = [];
      for (let indexValue of indexValues) {
        if (indexValue) {
          indexValuesToKeep.push(indexValue);
        }
      }
      for (let toKeep of indexValuesToKeep) {
        url.searchParams.append("index", toKeep);
      }
      return url;
    }
    function singleFetchUrl(reqUrl) {
      let url = typeof reqUrl === "string" ? new URL(
        reqUrl,
        // This can be called during the SSR flow via PrefetchPageLinksImpl so
        // don't assume window is available
        typeof window === "undefined" ? "server://singlefetch/" : window.location.origin
      ) : reqUrl;
      if (url.pathname === "/") {
        url.pathname = "_root.data";
      } else {
        url.pathname = `${url.pathname.replace(/\/$/, "")}.data`;
      }
      return url;
    }
    async function fetchAndDecode(url, init) {
      let res = await fetch(url, init);
      if (res.status === 404 && !res.headers.has("X-Remix-Response")) {
        throw new ErrorResponseImpl(404, "Not Found", true);
      }
      const NO_BODY_STATUS_CODES = /* @__PURE__ */ new Set([100, 101, 204, 205]);
      if (NO_BODY_STATUS_CODES.has(res.status)) {
        if (!init.method || init.method === "GET") {
          return { status: res.status, data: {} };
        } else {
          return { status: res.status, data: { data: void 0 } };
        }
      }
      invariant2(res.body, "No response body to decode");
      try {
        let decoded = await decodeViaTurboStream(res.body, window);
        return { status: res.status, data: decoded.value };
      } catch (e) {
        throw new Error("Unable to decode turbo-stream response");
      }
    }
    function decodeViaTurboStream(body, global2) {
      return (0, import_turbo_stream.decode)(body, {
        plugins: [
          (type, ...rest) => {
            if (type === "SanitizedError") {
              let [name, message, stack] = rest;
              let Constructor = Error;
              if (name && name in global2 && typeof global2[name] === "function") {
                Constructor = global2[name];
              }
              let error = new Constructor(message);
              error.stack = stack;
              return { value: error };
            }
            if (type === "ErrorResponse") {
              let [data2, status, statusText] = rest;
              return {
                value: new ErrorResponseImpl(status, statusText, data2)
              };
            }
            if (type === "SingleFetchRedirect") {
              return { value: { [SingleFetchRedirectSymbol]: rest[0] } };
            }
            if (type === "SingleFetchClassInstance") {
              return { value: rest[0] };
            }
            if (type === "SingleFetchFallback") {
              return { value: void 0 };
            }
          }
        ]
      });
    }
    function unwrapSingleFetchResults(results, routeId) {
      let redirect2 = results[SingleFetchRedirectSymbol];
      if (redirect2) {
        return unwrapSingleFetchResult(redirect2, routeId);
      }
      return results[routeId] !== void 0 ? unwrapSingleFetchResult(results[routeId], routeId) : null;
    }
    function unwrapSingleFetchResult(result, routeId) {
      if ("error" in result) {
        throw result.error;
      } else if ("redirect" in result) {
        let headers = {};
        if (result.revalidate) {
          headers["X-Remix-Revalidate"] = "yes";
        }
        if (result.reload) {
          headers["X-Remix-Reload-Document"] = "yes";
        }
        if (result.replace) {
          headers["X-Remix-Replace"] = "yes";
        }
        throw redirect(result.redirect, { status: result.status, headers });
      } else if ("data" in result) {
        return result.data;
      } else {
        throw new Error(`No response found for routeId "${routeId}"`);
      }
    }
    function createDeferred2() {
      let resolve;
      let reject;
      let promise = new Promise((res, rej) => {
        resolve = async (val) => {
          res(val);
          try {
            await promise;
          } catch (e) {
          }
        };
        reject = async (error) => {
          rej(error);
          try {
            await promise;
          } catch (e) {
          }
        };
      });
      return {
        promise,
        //@ts-ignore
        resolve,
        //@ts-ignore
        reject
      };
    }
    var React8 = __toESM2(require_react());
    var React7 = __toESM2(require_react());
    var React5 = __toESM2(require_react());
    var RemixErrorBoundary = class extends React5.Component {
      constructor(props) {
        super(props);
        this.state = { error: props.error || null, location: props.location };
      }
      static getDerivedStateFromError(error) {
        return { error };
      }
      static getDerivedStateFromProps(props, state) {
        if (state.location !== props.location) {
          return { error: props.error || null, location: props.location };
        }
        return { error: props.error || state.error, location: state.location };
      }
      render() {
        if (this.state.error) {
          return React5.createElement(
            RemixRootDefaultErrorBoundary,
            {
              error: this.state.error,
              isOutsideRemixApp: true
            }
          );
        } else {
          return this.props.children;
        }
      }
    };
    function RemixRootDefaultErrorBoundary({
      error,
      isOutsideRemixApp
    }) {
      console.error(error);
      let heyDeveloper = React5.createElement(
        "script",
        {
          dangerouslySetInnerHTML: {
            __html: `
        console.log(
          "💿 Hey developer 👋. You can provide a way better UX than this when your app throws errors. Check out https://remix.run/guides/errors for more information."
        );
      `
          }
        }
      );
      if (isRouteErrorResponse(error)) {
        return React5.createElement(BoundaryShell, { title: "Unhandled Thrown Response!" }, React5.createElement("h1", { style: { fontSize: "24px" } }, error.status, " ", error.statusText), heyDeveloper);
      }
      let errorInstance;
      if (error instanceof Error) {
        errorInstance = error;
      } else {
        let errorString = error == null ? "Unknown Error" : typeof error === "object" && "toString" in error ? error.toString() : JSON.stringify(error);
        errorInstance = new Error(errorString);
      }
      return React5.createElement(
        BoundaryShell,
        {
          title: "Application Error!",
          isOutsideRemixApp
        },
        React5.createElement("h1", { style: { fontSize: "24px" } }, "Application Error"),
        React5.createElement(
          "pre",
          {
            style: {
              padding: "2rem",
              background: "hsla(10, 50%, 50%, 0.1)",
              color: "red",
              overflow: "auto"
            }
          },
          errorInstance.stack
        ),
        heyDeveloper
      );
    }
    function BoundaryShell({
      title,
      renderScripts,
      isOutsideRemixApp,
      children
    }) {
      var _a;
      let { routeModules } = useFrameworkContext();
      if (((_a = routeModules.root) == null ? void 0 : _a.Layout) && !isOutsideRemixApp) {
        return children;
      }
      return React5.createElement("html", { lang: "en" }, React5.createElement("head", null, React5.createElement("meta", { charSet: "utf-8" }), React5.createElement(
        "meta",
        {
          name: "viewport",
          content: "width=device-width,initial-scale=1,viewport-fit=cover"
        }
      ), React5.createElement("title", null, title)), React5.createElement("body", null, React5.createElement("main", { style: { fontFamily: "system-ui, sans-serif", padding: "2rem" } }, children, renderScripts ? React5.createElement(Scripts, null) : null)));
    }
    var React6 = __toESM2(require_react());
    function RemixRootDefaultHydrateFallback() {
      return React6.createElement(BoundaryShell, { title: "Loading...", renderScripts: true }, React6.createElement(
        "script",
        {
          dangerouslySetInnerHTML: {
            __html: `
              console.log(
                "💿 Hey developer 👋. You can provide a way better UX than this " +
                "when your app is loading JS modules and/or running \`clientLoader\` " +
                "functions. Check out https://remix.run/route/hydrate-fallback " +
                "for more information."
              );
            `
          }
        }
      ));
    }
    function groupRoutesByParentId(manifest) {
      let routes = {};
      Object.values(manifest).forEach((route) => {
        if (route) {
          let parentId = route.parentId || "";
          if (!routes[parentId]) {
            routes[parentId] = [];
          }
          routes[parentId].push(route);
        }
      });
      return routes;
    }
    function getRouteComponents(route, routeModule, isSpaMode) {
      let Component4 = getRouteModuleComponent(routeModule);
      let HydrateFallback = routeModule.HydrateFallback && (!isSpaMode || route.id === "root") ? routeModule.HydrateFallback : route.id === "root" ? RemixRootDefaultHydrateFallback : void 0;
      let ErrorBoundary = routeModule.ErrorBoundary ? routeModule.ErrorBoundary : route.id === "root" ? () => React7.createElement(RemixRootDefaultErrorBoundary, { error: useRouteError() }) : void 0;
      if (route.id === "root" && routeModule.Layout) {
        return {
          ...Component4 ? {
            element: React7.createElement(routeModule.Layout, null, React7.createElement(Component4, null))
          } : { Component: Component4 },
          ...ErrorBoundary ? {
            errorElement: React7.createElement(routeModule.Layout, null, React7.createElement(ErrorBoundary, null))
          } : { ErrorBoundary },
          ...HydrateFallback ? {
            hydrateFallbackElement: React7.createElement(routeModule.Layout, null, React7.createElement(HydrateFallback, null))
          } : { HydrateFallback }
        };
      }
      return { Component: Component4, ErrorBoundary, HydrateFallback };
    }
    function createClientRoutesWithHMRRevalidationOptOut(needsRevalidation, manifest, routeModulesCache, initialState, ssr, isSpaMode) {
      return createClientRoutes(
        manifest,
        routeModulesCache,
        initialState,
        ssr,
        isSpaMode,
        "",
        groupRoutesByParentId(manifest),
        needsRevalidation
      );
    }
    function preventInvalidServerHandlerCall(type, route) {
      if (type === "loader" && !route.hasLoader || type === "action" && !route.hasAction) {
        let fn = type === "action" ? "serverAction()" : "serverLoader()";
        let msg = `You are trying to call ${fn} on a route that does not have a server ${type} (routeId: "${route.id}")`;
        console.error(msg);
        throw new ErrorResponseImpl(400, "Bad Request", new Error(msg), true);
      }
    }
    function noActionDefinedError(type, routeId) {
      let article = type === "clientAction" ? "a" : "an";
      let msg = `Route "${routeId}" does not have ${article} ${type}, but you are trying to submit to it. To fix this, please add ${article} \`${type}\` function to the route`;
      console.error(msg);
      throw new ErrorResponseImpl(405, "Method Not Allowed", new Error(msg), true);
    }
    function createClientRoutes(manifest, routeModulesCache, initialState, ssr, isSpaMode, parentId = "", routesByParentId = groupRoutesByParentId(manifest), needsRevalidation) {
      return (routesByParentId[parentId] || []).map((route) => {
        var _a, _b, _c;
        let routeModule = routeModulesCache[route.id];
        function fetchServerHandler(singleFetch) {
          invariant2(
            typeof singleFetch === "function",
            "No single fetch function available for route handler"
          );
          return singleFetch();
        }
        function fetchServerLoader(singleFetch) {
          if (!route.hasLoader) return Promise.resolve(null);
          return fetchServerHandler(singleFetch);
        }
        function fetchServerAction(singleFetch) {
          if (!route.hasAction) {
            throw noActionDefinedError("action", route.id);
          }
          return fetchServerHandler(singleFetch);
        }
        function prefetchModule(modulePath) {
          import(
            /* @vite-ignore */
            /* webpackIgnore: true */
            modulePath
          );
        }
        function prefetchRouteModuleChunks(route2) {
          if (route2.clientActionModule) {
            prefetchModule(route2.clientActionModule);
          }
          if (route2.clientLoaderModule) {
            prefetchModule(route2.clientLoaderModule);
          }
        }
        async function prefetchStylesAndCallHandler(handler) {
          let cachedModule = routeModulesCache[route.id];
          let linkPrefetchPromise = cachedModule ? prefetchStyleLinks(route, cachedModule) : Promise.resolve();
          try {
            return handler();
          } finally {
            await linkPrefetchPromise;
          }
        }
        let dataRoute = {
          id: route.id,
          index: route.index,
          path: route.path
        };
        if (routeModule) {
          Object.assign(dataRoute, {
            ...dataRoute,
            ...getRouteComponents(route, routeModule, isSpaMode),
            handle: routeModule.handle,
            shouldRevalidate: getShouldRevalidateFunction(
              routeModule,
              route,
              ssr,
              needsRevalidation
            )
          });
          let hasInitialData = initialState && initialState.loaderData && route.id in initialState.loaderData;
          let initialData = hasInitialData ? (_a = initialState == null ? void 0 : initialState.loaderData) == null ? void 0 : _a[route.id] : void 0;
          let hasInitialError = initialState && initialState.errors && route.id in initialState.errors;
          let initialError = hasInitialError ? (_b = initialState == null ? void 0 : initialState.errors) == null ? void 0 : _b[route.id] : void 0;
          let isHydrationRequest = needsRevalidation == null && (((_c = routeModule.clientLoader) == null ? void 0 : _c.hydrate) === true || !route.hasLoader);
          dataRoute.loader = async ({ request, params }, singleFetch) => {
            try {
              let result = await prefetchStylesAndCallHandler(async () => {
                invariant2(
                  routeModule,
                  "No `routeModule` available for critical-route loader"
                );
                if (!routeModule.clientLoader) {
                  return fetchServerLoader(singleFetch);
                }
                return routeModule.clientLoader({
                  request,
                  params,
                  async serverLoader() {
                    preventInvalidServerHandlerCall("loader", route);
                    if (isHydrationRequest) {
                      if (hasInitialData) {
                        return initialData;
                      }
                      if (hasInitialError) {
                        throw initialError;
                      }
                    }
                    return fetchServerLoader(singleFetch);
                  }
                });
              });
              return result;
            } finally {
              isHydrationRequest = false;
            }
          };
          dataRoute.loader.hydrate = shouldHydrateRouteLoader(
            route,
            routeModule,
            isSpaMode
          );
          dataRoute.action = ({ request, params }, singleFetch) => {
            return prefetchStylesAndCallHandler(async () => {
              invariant2(
                routeModule,
                "No `routeModule` available for critical-route action"
              );
              if (!routeModule.clientAction) {
                if (isSpaMode) {
                  throw noActionDefinedError("clientAction", route.id);
                }
                return fetchServerAction(singleFetch);
              }
              return routeModule.clientAction({
                request,
                params,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            });
          };
        } else {
          if (!route.hasClientLoader) {
            dataRoute.loader = ({ request }, singleFetch) => prefetchStylesAndCallHandler(() => {
              return fetchServerLoader(singleFetch);
            });
          } else if (route.clientLoaderModule) {
            dataRoute.loader = async (args, singleFetch) => {
              invariant2(route.clientLoaderModule);
              let { clientLoader } = await import(
                /* @vite-ignore */
                /* webpackIgnore: true */
                route.clientLoaderModule
              );
              return clientLoader({
                ...args,
                async serverLoader() {
                  preventInvalidServerHandlerCall("loader", route);
                  return fetchServerLoader(singleFetch);
                }
              });
            };
          }
          if (!route.hasClientAction) {
            dataRoute.action = ({ request }, singleFetch) => prefetchStylesAndCallHandler(() => {
              if (isSpaMode) {
                throw noActionDefinedError("clientAction", route.id);
              }
              return fetchServerAction(singleFetch);
            });
          } else if (route.clientActionModule) {
            dataRoute.action = async (args, singleFetch) => {
              invariant2(route.clientActionModule);
              prefetchRouteModuleChunks(route);
              let { clientAction } = await import(
                /* @vite-ignore */
                /* webpackIgnore: true */
                route.clientActionModule
              );
              return clientAction({
                ...args,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            };
          }
          dataRoute.lazy = async () => {
            if (route.clientLoaderModule || route.clientActionModule) {
              await new Promise((resolve) => setTimeout(resolve, 0));
            }
            let modPromise = loadRouteModuleWithBlockingLinks(
              route,
              routeModulesCache
            );
            prefetchRouteModuleChunks(route);
            let mod = await modPromise;
            let lazyRoute = { ...mod };
            if (mod.clientLoader) {
              let clientLoader = mod.clientLoader;
              lazyRoute.loader = (args, singleFetch) => clientLoader({
                ...args,
                async serverLoader() {
                  preventInvalidServerHandlerCall("loader", route);
                  return fetchServerLoader(singleFetch);
                }
              });
            }
            if (mod.clientAction) {
              let clientAction = mod.clientAction;
              lazyRoute.action = (args, singleFetch) => clientAction({
                ...args,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            }
            return {
              ...lazyRoute.loader ? { loader: lazyRoute.loader } : {},
              ...lazyRoute.action ? { action: lazyRoute.action } : {},
              hasErrorBoundary: lazyRoute.hasErrorBoundary,
              shouldRevalidate: getShouldRevalidateFunction(
                lazyRoute,
                route,
                ssr,
                needsRevalidation
              ),
              handle: lazyRoute.handle,
              // No need to wrap these in layout since the root route is never
              // loaded via route.lazy()
              Component: lazyRoute.Component,
              ErrorBoundary: lazyRoute.ErrorBoundary
            };
          };
        }
        let children = createClientRoutes(
          manifest,
          routeModulesCache,
          initialState,
          ssr,
          isSpaMode,
          route.id,
          routesByParentId,
          needsRevalidation
        );
        if (children.length > 0) dataRoute.children = children;
        return dataRoute;
      });
    }
    function getShouldRevalidateFunction(route, manifestRoute, ssr, needsRevalidation) {
      if (needsRevalidation) {
        return wrapShouldRevalidateForHdr(
          manifestRoute.id,
          route.shouldRevalidate,
          needsRevalidation
        );
      }
      if (!ssr && manifestRoute.hasLoader && !manifestRoute.hasClientLoader) {
        if (route.shouldRevalidate) {
          let fn = route.shouldRevalidate;
          return (opts) => fn({ ...opts, defaultShouldRevalidate: false });
        } else {
          return () => false;
        }
      }
      if (ssr && route.shouldRevalidate) {
        let fn = route.shouldRevalidate;
        return (opts) => fn({ ...opts, defaultShouldRevalidate: true });
      }
      return route.shouldRevalidate;
    }
    function wrapShouldRevalidateForHdr(routeId, routeShouldRevalidate, needsRevalidation) {
      let handledRevalidation = false;
      return (arg) => {
        if (!handledRevalidation) {
          handledRevalidation = true;
          return needsRevalidation.has(routeId);
        }
        return routeShouldRevalidate ? routeShouldRevalidate(arg) : arg.defaultShouldRevalidate;
      };
    }
    async function loadRouteModuleWithBlockingLinks(route, routeModules) {
      let routeModulePromise = loadRouteModule(route, routeModules);
      let prefetchRouteCssPromise = prefetchRouteCss(route);
      let routeModule = await routeModulePromise;
      await Promise.all([
        prefetchRouteCssPromise,
        prefetchStyleLinks(route, routeModule)
      ]);
      return {
        Component: getRouteModuleComponent(routeModule),
        ErrorBoundary: routeModule.ErrorBoundary,
        clientAction: routeModule.clientAction,
        clientLoader: routeModule.clientLoader,
        handle: routeModule.handle,
        links: routeModule.links,
        meta: routeModule.meta,
        shouldRevalidate: routeModule.shouldRevalidate
      };
    }
    function getRouteModuleComponent(routeModule) {
      if (routeModule.default == null) return void 0;
      let isEmptyObject = typeof routeModule.default === "object" && Object.keys(routeModule.default).length === 0;
      if (!isEmptyObject) {
        return routeModule.default;
      }
    }
    function shouldHydrateRouteLoader(route, routeModule, isSpaMode) {
      return isSpaMode && route.id !== "root" || routeModule.clientLoader != null && (routeModule.clientLoader.hydrate === true || route.hasLoader !== true);
    }
    var nextPaths = /* @__PURE__ */ new Set();
    var discoveredPathsMaxSize = 1e3;
    var discoveredPaths = /* @__PURE__ */ new Set();
    var URL_LIMIT = 7680;
    function isFogOfWarEnabled(ssr) {
      return ssr === true;
    }
    function getPartialManifest(manifest, router2) {
      let routeIds = new Set(router2.state.matches.map((m) => m.route.id));
      let segments = router2.state.location.pathname.split("/").filter(Boolean);
      let paths = ["/"];
      segments.pop();
      while (segments.length > 0) {
        paths.push(`/${segments.join("/")}`);
        segments.pop();
      }
      paths.forEach((path) => {
        let matches = matchRoutes(router2.routes, path, router2.basename);
        if (matches) {
          matches.forEach((m) => routeIds.add(m.route.id));
        }
      });
      let initialRoutes = [...routeIds].reduce(
        (acc, id) => Object.assign(acc, { [id]: manifest.routes[id] }),
        {}
      );
      return {
        ...manifest,
        routes: initialRoutes
      };
    }
    function getPatchRoutesOnNavigationFunction(manifest, routeModules, ssr, isSpaMode, basename) {
      if (!isFogOfWarEnabled(ssr)) {
        return void 0;
      }
      return async ({ path, patch, signal }) => {
        if (discoveredPaths.has(path)) {
          return;
        }
        await fetchAndApplyManifestPatches(
          [path],
          manifest,
          routeModules,
          ssr,
          isSpaMode,
          basename,
          patch,
          signal
        );
      };
    }
    function useFogOFWarDiscovery(router2, manifest, routeModules, ssr, isSpaMode) {
      React8.useEffect(() => {
        var _a;
        if (!isFogOfWarEnabled(ssr) || ((_a = navigator.connection) == null ? void 0 : _a.saveData) === true) {
          return;
        }
        function registerElement(el) {
          let path = el.tagName === "FORM" ? el.getAttribute("action") : el.getAttribute("href");
          if (!path) {
            return;
          }
          let pathname = el.tagName === "A" ? el.pathname : new URL(path, window.location.origin).pathname;
          if (!discoveredPaths.has(pathname)) {
            nextPaths.add(pathname);
          }
        }
        async function fetchPatches() {
          document.querySelectorAll("a[data-discover], form[data-discover]").forEach(registerElement);
          let lazyPaths = Array.from(nextPaths.keys()).filter((path) => {
            if (discoveredPaths.has(path)) {
              nextPaths.delete(path);
              return false;
            }
            return true;
          });
          if (lazyPaths.length === 0) {
            return;
          }
          try {
            await fetchAndApplyManifestPatches(
              lazyPaths,
              manifest,
              routeModules,
              ssr,
              isSpaMode,
              router2.basename,
              router2.patchRoutes
            );
          } catch (e) {
            console.error("Failed to fetch manifest patches", e);
          }
        }
        let debouncedFetchPatches = debounce(fetchPatches, 100);
        fetchPatches();
        let observer = new MutationObserver(() => debouncedFetchPatches());
        observer.observe(document.documentElement, {
          subtree: true,
          childList: true,
          attributes: true,
          attributeFilter: ["data-discover", "href", "action"]
        });
        return () => observer.disconnect();
      }, [ssr, isSpaMode, manifest, routeModules, router2]);
    }
    async function fetchAndApplyManifestPatches(paths, manifest, routeModules, ssr, isSpaMode, basename, patchRoutes, signal) {
      let manifestPath = `${basename != null ? basename : "/"}/__manifest`.replace(
        /\/+/g,
        "/"
      );
      let url = new URL(manifestPath, window.location.origin);
      paths.sort().forEach((path) => url.searchParams.append("p", path));
      url.searchParams.set("version", manifest.version);
      if (url.toString().length > URL_LIMIT) {
        nextPaths.clear();
        return;
      }
      let serverPatches;
      try {
        let res = await fetch(url, { signal });
        if (!res.ok) {
          throw new Error(`${res.status} ${res.statusText}`);
        } else if (res.status >= 400) {
          throw new Error(await res.text());
        }
        serverPatches = await res.json();
      } catch (e) {
        if (signal == null ? void 0 : signal.aborted) return;
        throw e;
      }
      let knownRoutes = new Set(Object.keys(manifest.routes));
      let patches = Object.values(serverPatches).reduce((acc, route) => {
        if (route && !knownRoutes.has(route.id)) {
          acc[route.id] = route;
        }
        return acc;
      }, {});
      Object.assign(manifest.routes, patches);
      paths.forEach((p) => addToFifoQueue(p, discoveredPaths));
      let parentIds = /* @__PURE__ */ new Set();
      Object.values(patches).forEach((patch) => {
        if (patch && (!patch.parentId || !patches[patch.parentId])) {
          parentIds.add(patch.parentId);
        }
      });
      parentIds.forEach(
        (parentId) => patchRoutes(
          parentId || null,
          createClientRoutes(patches, routeModules, null, ssr, isSpaMode, parentId)
        )
      );
    }
    function addToFifoQueue(path, queue) {
      if (queue.size >= discoveredPathsMaxSize) {
        let first = queue.values().next().value;
        queue.delete(first);
      }
      queue.add(path);
    }
    function debounce(callback, wait) {
      let timeoutId;
      return (...args) => {
        window.clearTimeout(timeoutId);
        timeoutId = window.setTimeout(() => callback(...args), wait);
      };
    }
    function useDataRouterContext() {
      let context = React9.useContext(DataRouterContext);
      invariant2(
        context,
        "You must render this element inside a <DataRouterContext.Provider> element"
      );
      return context;
    }
    function useDataRouterStateContext() {
      let context = React9.useContext(DataRouterStateContext);
      invariant2(
        context,
        "You must render this element inside a <DataRouterStateContext.Provider> element"
      );
      return context;
    }
    var FrameworkContext = React9.createContext(void 0);
    FrameworkContext.displayName = "FrameworkContext";
    function useFrameworkContext() {
      let context = React9.useContext(FrameworkContext);
      invariant2(
        context,
        "You must render this element inside a <HydratedRouter> element"
      );
      return context;
    }
    function getActiveMatches(matches, errors, isSpaMode) {
      if (isSpaMode && !isHydrated) {
        return [matches[0]];
      }
      if (errors) {
        let errorIdx = matches.findIndex((m) => errors[m.route.id] !== void 0);
        return matches.slice(0, errorIdx + 1);
      }
      return matches;
    }
    var isHydrated = false;
    function Scripts(props) {
      let { manifest, serverHandoffString, isSpaMode, ssr, renderMeta } = useFrameworkContext();
      let { router: router2, static: isStatic, staticContext } = useDataRouterContext();
      let { matches: routerMatches } = useDataRouterStateContext();
      let enableFogOfWar = isFogOfWarEnabled(ssr);
      if (renderMeta) {
        renderMeta.didRenderScripts = true;
      }
      let matches = getActiveMatches(routerMatches, null, isSpaMode);
      React9.useEffect(() => {
        isHydrated = true;
      }, []);
      let initialScripts = React9.useMemo(() => {
        var _a;
        let streamScript = "window.__reactRouterContext.stream = new ReadableStream({start(controller){window.__reactRouterContext.streamController = controller;}}).pipeThrough(new TextEncoderStream());";
        let contextScript = staticContext ? `window.__reactRouterContext = ${serverHandoffString};${streamScript}` : " ";
        let routeModulesScript = !isStatic ? " " : `${((_a = manifest.hmr) == null ? void 0 : _a.runtime) ? `import ${JSON.stringify(manifest.hmr.runtime)};` : ""}${!enableFogOfWar ? `import ${JSON.stringify(manifest.url)}` : ""};
${matches.map((match, routeIndex) => {
          let routeVarName = `route${routeIndex}`;
          let manifestEntry = manifest.routes[match.route.id];
          invariant2(manifestEntry, `Route ${match.route.id} not found in manifest`);
          let {
            clientActionModule,
            clientLoaderModule,
            hydrateFallbackModule,
            module: module2
          } = manifestEntry;
          let chunks = [
            ...clientActionModule ? [
              {
                module: clientActionModule,
                varName: `${routeVarName}_clientAction`
              }
            ] : [],
            ...clientLoaderModule ? [
              {
                module: clientLoaderModule,
                varName: `${routeVarName}_clientLoader`
              }
            ] : [],
            ...hydrateFallbackModule ? [
              {
                module: hydrateFallbackModule,
                varName: `${routeVarName}_HydrateFallback`
              }
            ] : [],
            { module: module2, varName: `${routeVarName}_main` }
          ];
          if (chunks.length === 1) {
            return `import * as ${routeVarName} from ${JSON.stringify(module2)};`;
          }
          let chunkImportsSnippet = chunks.map((chunk) => `import * as ${chunk.varName} from "${chunk.module}";`).join("\n");
          let mergedChunksSnippet = `const ${routeVarName} = {${chunks.map((chunk) => `...${chunk.varName}`).join(",")}};`;
          return [chunkImportsSnippet, mergedChunksSnippet].join("\n");
        }).join("\n")}
  ${enableFogOfWar ? (
          // Inline a minimal manifest with the SSR matches
          `window.__reactRouterManifest = ${JSON.stringify(
            getPartialManifest(manifest, router2),
            null,
            2
          )};`
        ) : ""}
  window.__reactRouterRouteModules = {${matches.map((match, index) => `${JSON.stringify(match.route.id)}:route${index}`).join(",")}};

import(${JSON.stringify(manifest.entry.module)});`;
        return React9.createElement(React9.Fragment, null, React9.createElement(
          "script",
          {
            ...props,
            suppressHydrationWarning: true,
            dangerouslySetInnerHTML: createHtml(contextScript),
            type: void 0
          }
        ), React9.createElement(
          "script",
          {
            ...props,
            suppressHydrationWarning: true,
            dangerouslySetInnerHTML: createHtml(routeModulesScript),
            type: "module",
            async: true
          }
        ));
      }, []);
      let preloads = isHydrated ? [] : manifest.entry.imports.concat(
        getModuleLinkHrefs(matches, manifest, {
          includeHydrateFallback: true
        })
      );
      return isHydrated ? null : React9.createElement(React9.Fragment, null, !enableFogOfWar ? React9.createElement(
        "link",
        {
          rel: "modulepreload",
          href: manifest.url,
          crossOrigin: props.crossOrigin
        }
      ) : null, React9.createElement(
        "link",
        {
          rel: "modulepreload",
          href: manifest.entry.module,
          crossOrigin: props.crossOrigin
        }
      ), dedupe(preloads).map((path) => React9.createElement(
        "link",
        {
          key: path,
          rel: "modulepreload",
          href: path,
          crossOrigin: props.crossOrigin
        }
      )), initialScripts);
    }
    function dedupe(array) {
      return [...new Set(array)];
    }
    function deserializeErrors(errors) {
      if (!errors) return null;
      let entries = Object.entries(errors);
      let serialized = {};
      for (let [key, val] of entries) {
        if (val && val.__type === "RouteErrorResponse") {
          serialized[key] = new ErrorResponseImpl(
            val.status,
            val.statusText,
            val.data,
            val.internal === true
          );
        } else if (val && val.__type === "Error") {
          if (val.__subType) {
            let ErrorConstructor = window[val.__subType];
            if (typeof ErrorConstructor === "function") {
              try {
                let error = new ErrorConstructor(val.message);
                error.stack = val.stack;
                serialized[key] = error;
              } catch (e) {
              }
            }
          }
          if (serialized[key] == null) {
            let error = new Error(val.message);
            error.stack = val.stack;
            serialized[key] = error;
          }
        } else {
          serialized[key] = val;
        }
      }
      return serialized;
    }
    function RouterProvider22(props) {
      return React10.createElement(RouterProvider2, { flushSync: ReactDOM.flushSync, ...props });
    }
    var React11 = __toESM2(require_react());
    var ssrInfo = null;
    var router = null;
    function initSsrInfo() {
      if (!ssrInfo && window.__reactRouterContext && window.__reactRouterManifest && window.__reactRouterRouteModules) {
        ssrInfo = {
          context: window.__reactRouterContext,
          manifest: window.__reactRouterManifest,
          routeModules: window.__reactRouterRouteModules,
          stateDecodingPromise: void 0,
          router: void 0,
          routerInitialized: false
        };
      }
    }
    function createHydratedRouter() {
      var _a;
      initSsrInfo();
      if (!ssrInfo) {
        throw new Error(
          "You must be using the SSR features of React Router in order to skip passing a `router` prop to `<RouterProvider>`"
        );
      }
      let localSsrInfo = ssrInfo;
      if (!ssrInfo.stateDecodingPromise) {
        let stream = ssrInfo.context.stream;
        invariant(stream, "No stream found for single fetch decoding");
        ssrInfo.context.stream = void 0;
        ssrInfo.stateDecodingPromise = decodeViaTurboStream(stream, window).then((value) => {
          ssrInfo.context.state = value.value;
          localSsrInfo.stateDecodingPromise.value = true;
        }).catch((e) => {
          localSsrInfo.stateDecodingPromise.error = e;
        });
      }
      if (ssrInfo.stateDecodingPromise.error) {
        throw ssrInfo.stateDecodingPromise.error;
      }
      if (!ssrInfo.stateDecodingPromise.value) {
        throw ssrInfo.stateDecodingPromise;
      }
      let routes = createClientRoutes(
        ssrInfo.manifest.routes,
        ssrInfo.routeModules,
        ssrInfo.context.state,
        ssrInfo.context.ssr,
        ssrInfo.context.isSpaMode
      );
      let hydrationData = void 0;
      let loaderData = ssrInfo.context.state.loaderData;
      if (ssrInfo.context.isSpaMode) {
        hydrationData = { loaderData };
      } else {
        hydrationData = {
          ...ssrInfo.context.state,
          loaderData: { ...loaderData }
        };
        let initialMatches = matchRoutes(
          routes,
          window.location,
          (_a = window.__reactRouterContext) == null ? void 0 : _a.basename
        );
        if (initialMatches) {
          for (let match of initialMatches) {
            let routeId = match.route.id;
            let route = ssrInfo.routeModules[routeId];
            let manifestRoute = ssrInfo.manifest.routes[routeId];
            if (route && manifestRoute && shouldHydrateRouteLoader(
              manifestRoute,
              route,
              ssrInfo.context.isSpaMode
            ) && (route.HydrateFallback || !manifestRoute.hasLoader)) {
              delete hydrationData.loaderData[routeId];
            } else if (manifestRoute && !manifestRoute.hasLoader) {
              hydrationData.loaderData[routeId] = null;
            }
          }
        }
        if (hydrationData && hydrationData.errors) {
          hydrationData.errors = deserializeErrors(hydrationData.errors);
        }
      }
      let router2 = createRouter({
        routes,
        history: createBrowserHistory(),
        basename: ssrInfo.context.basename,
        hydrationData,
        mapRouteProperties,
        dataStrategy: getSingleFetchDataStrategy(
          ssrInfo.manifest,
          ssrInfo.routeModules,
          ssrInfo.context.ssr,
          () => router2
        ),
        patchRoutesOnNavigation: getPatchRoutesOnNavigationFunction(
          ssrInfo.manifest,
          ssrInfo.routeModules,
          ssrInfo.context.ssr,
          ssrInfo.context.isSpaMode,
          ssrInfo.context.basename
        )
      });
      ssrInfo.router = router2;
      if (router2.state.initialized) {
        ssrInfo.routerInitialized = true;
        router2.initialize();
      }
      router2.createRoutesForHMR = /* spacer so ts-ignore does not affect the right hand of the assignment */
      createClientRoutesWithHMRRevalidationOptOut;
      window.__reactRouterDataRouter = router2;
      return router2;
    }
    function HydratedRouter2() {
      if (!router) {
        router = createHydratedRouter();
      }
      let [criticalCss, setCriticalCss] = React11.useState(
        true ? ssrInfo == null ? void 0 : ssrInfo.context.criticalCss : void 0
      );
      if (true) {
        if (ssrInfo) {
          window.__reactRouterClearCriticalCss = () => setCriticalCss(void 0);
        }
      }
      let [location, setLocation] = React11.useState(router.state.location);
      React11.useLayoutEffect(() => {
        if (ssrInfo && ssrInfo.router && !ssrInfo.routerInitialized) {
          ssrInfo.routerInitialized = true;
          ssrInfo.router.initialize();
        }
      }, []);
      React11.useLayoutEffect(() => {
        if (ssrInfo && ssrInfo.router) {
          return ssrInfo.router.subscribe((newState) => {
            if (newState.location !== location) {
              setLocation(newState.location);
            }
          });
        }
      }, [location]);
      invariant(ssrInfo, "ssrInfo unavailable for HydratedRouter");
      useFogOFWarDiscovery(
        router,
        ssrInfo.manifest,
        ssrInfo.routeModules,
        ssrInfo.context.ssr,
        ssrInfo.context.isSpaMode
      );
      return (
        // This fragment is important to ensure we match the <ServerRouter> JSX
        // structure so that useId values hydrate correctly
        React11.createElement(React11.Fragment, null, React11.createElement(
          FrameworkContext.Provider,
          {
            value: {
              manifest: ssrInfo.manifest,
              routeModules: ssrInfo.routeModules,
              future: ssrInfo.context.future,
              criticalCss,
              ssr: ssrInfo.context.ssr,
              isSpaMode: ssrInfo.context.isSpaMode
            }
          },
          React11.createElement(RemixErrorBoundary, { location }, React11.createElement(RouterProvider22, { router }))
        ), React11.createElement(React11.Fragment, null))
      );
    }
  }
});

// node_modules/react-router/dist/development/index.js
var require_development = __commonJS({
  "node_modules/react-router/dist/development/index.js"(exports, module) {
    "use strict";
    var __create = Object.create;
    var __defProp = Object.defineProperty;
    var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
    var __getOwnPropNames = Object.getOwnPropertyNames;
    var __getProtoOf = Object.getPrototypeOf;
    var __hasOwnProp = Object.prototype.hasOwnProperty;
    var __export2 = (target, all) => {
      for (var name in all)
        __defProp(target, name, { get: all[name], enumerable: true });
    };
    var __copyProps = (to, from, except, desc) => {
      if (from && typeof from === "object" || typeof from === "function") {
        for (let key of __getOwnPropNames(from))
          if (!__hasOwnProp.call(to, key) && key !== except)
            __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
      }
      return to;
    };
    var __toESM2 = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
      // If the importer is in node compatibility mode or this is not an ESM
      // file that has been converted to a CommonJS file using a Babel-
      // compatible transform (i.e. "__esModule" has not been set), then set
      // "default" to the CommonJS "module.exports" for node compatibility.
      isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
      mod
    ));
    var __toCommonJS2 = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
    var react_router_exports = {};
    __export2(react_router_exports, {
      Await: () => Await,
      BrowserRouter: () => BrowserRouter,
      Form: () => Form,
      HashRouter: () => HashRouter,
      IDLE_BLOCKER: () => IDLE_BLOCKER,
      IDLE_FETCHER: () => IDLE_FETCHER,
      IDLE_NAVIGATION: () => IDLE_NAVIGATION,
      Link: () => Link,
      Links: () => Links,
      MemoryRouter: () => MemoryRouter,
      Meta: () => Meta,
      NavLink: () => NavLink,
      Navigate: () => Navigate,
      NavigationType: () => Action,
      Outlet: () => Outlet,
      PrefetchPageLinks: () => PrefetchPageLinks,
      Route: () => Route,
      Router: () => Router,
      RouterProvider: () => RouterProvider2,
      Routes: () => Routes,
      Scripts: () => Scripts,
      ScrollRestoration: () => ScrollRestoration,
      ServerRouter: () => ServerRouter,
      StaticRouter: () => StaticRouter,
      StaticRouterProvider: () => StaticRouterProvider,
      UNSAFE_DataRouterContext: () => DataRouterContext,
      UNSAFE_DataRouterStateContext: () => DataRouterStateContext,
      UNSAFE_ErrorResponseImpl: () => ErrorResponseImpl,
      UNSAFE_FetchersContext: () => FetchersContext,
      UNSAFE_FrameworkContext: () => FrameworkContext,
      UNSAFE_LocationContext: () => LocationContext,
      UNSAFE_NavigationContext: () => NavigationContext,
      UNSAFE_RemixErrorBoundary: () => RemixErrorBoundary,
      UNSAFE_RouteContext: () => RouteContext,
      UNSAFE_ServerMode: () => ServerMode,
      UNSAFE_SingleFetchRedirectSymbol: () => SingleFetchRedirectSymbol,
      UNSAFE_ViewTransitionContext: () => ViewTransitionContext,
      UNSAFE_createBrowserHistory: () => createBrowserHistory,
      UNSAFE_createClientRoutes: () => createClientRoutes,
      UNSAFE_createClientRoutesWithHMRRevalidationOptOut: () => createClientRoutesWithHMRRevalidationOptOut,
      UNSAFE_createRouter: () => createRouter,
      UNSAFE_decodeViaTurboStream: () => decodeViaTurboStream,
      UNSAFE_deserializeErrors: () => deserializeErrors2,
      UNSAFE_getPatchRoutesOnNavigationFunction: () => getPatchRoutesOnNavigationFunction,
      UNSAFE_getSingleFetchDataStrategy: () => getSingleFetchDataStrategy,
      UNSAFE_invariant: () => invariant,
      UNSAFE_mapRouteProperties: () => mapRouteProperties,
      UNSAFE_shouldHydrateRouteLoader: () => shouldHydrateRouteLoader,
      UNSAFE_useFogOFWarDiscovery: () => useFogOFWarDiscovery,
      UNSAFE_useScrollRestoration: () => useScrollRestoration,
      createBrowserRouter: () => createBrowserRouter,
      createCookie: () => createCookie,
      createCookieSessionStorage: () => createCookieSessionStorage,
      createHashRouter: () => createHashRouter,
      createMemoryRouter: () => createMemoryRouter,
      createMemorySessionStorage: () => createMemorySessionStorage,
      createPath: () => createPath,
      createRequestHandler: () => createRequestHandler,
      createRoutesFromChildren: () => createRoutesFromChildren,
      createRoutesFromElements: () => createRoutesFromElements,
      createRoutesStub: () => createRoutesStub,
      createSearchParams: () => createSearchParams,
      createSession: () => createSession,
      createSessionStorage: () => createSessionStorage,
      createStaticHandler: () => createStaticHandler2,
      createStaticRouter: () => createStaticRouter,
      data: () => data,
      generatePath: () => generatePath,
      href: () => href,
      isCookie: () => isCookie,
      isRouteErrorResponse: () => isRouteErrorResponse,
      isSession: () => isSession,
      matchPath: () => matchPath,
      matchRoutes: () => matchRoutes,
      parsePath: () => parsePath,
      redirect: () => redirect,
      redirectDocument: () => redirectDocument,
      renderMatches: () => renderMatches,
      replace: () => replace,
      resolvePath: () => resolvePath,
      unstable_HistoryRouter: () => HistoryRouter,
      unstable_setDevServerHooks: () => setDevServerHooks,
      unstable_usePrompt: () => usePrompt,
      useActionData: () => useActionData,
      useAsyncError: () => useAsyncError,
      useAsyncValue: () => useAsyncValue,
      useBeforeUnload: () => useBeforeUnload,
      useBlocker: () => useBlocker,
      useFetcher: () => useFetcher,
      useFetchers: () => useFetchers,
      useFormAction: () => useFormAction,
      useHref: () => useHref,
      useInRouterContext: () => useInRouterContext,
      useLinkClickHandler: () => useLinkClickHandler,
      useLoaderData: () => useLoaderData,
      useLocation: () => useLocation,
      useMatch: () => useMatch,
      useMatches: () => useMatches,
      useNavigate: () => useNavigate,
      useNavigation: () => useNavigation,
      useNavigationType: () => useNavigationType,
      useOutlet: () => useOutlet,
      useOutletContext: () => useOutletContext,
      useParams: () => useParams,
      useResolvedPath: () => useResolvedPath,
      useRevalidator: () => useRevalidator,
      useRouteError: () => useRouteError,
      useRouteLoaderData: () => useRouteLoaderData,
      useRoutes: () => useRoutes,
      useSearchParams: () => useSearchParams,
      useSubmit: () => useSubmit,
      useViewTransitionState: () => useViewTransitionState
    });
    module.exports = __toCommonJS2(react_router_exports);
    var Action = ((Action2) => {
      Action2["Pop"] = "POP";
      Action2["Push"] = "PUSH";
      Action2["Replace"] = "REPLACE";
      return Action2;
    })(Action || {});
    var PopStateEventType = "popstate";
    function createMemoryHistory(options = {}) {
      let { initialEntries = ["/"], initialIndex, v5Compat = false } = options;
      let entries;
      entries = initialEntries.map(
        (entry, index2) => createMemoryLocation(
          entry,
          typeof entry === "string" ? null : entry.state,
          index2 === 0 ? "default" : void 0
        )
      );
      let index = clampIndex(
        initialIndex == null ? entries.length - 1 : initialIndex
      );
      let action = "POP";
      let listener = null;
      function clampIndex(n) {
        return Math.min(Math.max(n, 0), entries.length - 1);
      }
      function getCurrentLocation() {
        return entries[index];
      }
      function createMemoryLocation(to, state = null, key) {
        let location = createLocation(
          entries ? getCurrentLocation().pathname : "/",
          to,
          state,
          key
        );
        warning(
          location.pathname.charAt(0) === "/",
          `relative pathnames are not supported in memory history: ${JSON.stringify(
            to
          )}`
        );
        return location;
      }
      function createHref2(to) {
        return typeof to === "string" ? to : createPath(to);
      }
      let history = {
        get index() {
          return index;
        },
        get action() {
          return action;
        },
        get location() {
          return getCurrentLocation();
        },
        createHref: createHref2,
        createURL(to) {
          return new URL(createHref2(to), "http://localhost");
        },
        encodeLocation(to) {
          let path = typeof to === "string" ? parsePath(to) : to;
          return {
            pathname: path.pathname || "",
            search: path.search || "",
            hash: path.hash || ""
          };
        },
        push(to, state) {
          action = "PUSH";
          let nextLocation = createMemoryLocation(to, state);
          index += 1;
          entries.splice(index, entries.length, nextLocation);
          if (v5Compat && listener) {
            listener({ action, location: nextLocation, delta: 1 });
          }
        },
        replace(to, state) {
          action = "REPLACE";
          let nextLocation = createMemoryLocation(to, state);
          entries[index] = nextLocation;
          if (v5Compat && listener) {
            listener({ action, location: nextLocation, delta: 0 });
          }
        },
        go(delta) {
          action = "POP";
          let nextIndex = clampIndex(index + delta);
          let nextLocation = entries[nextIndex];
          index = nextIndex;
          if (listener) {
            listener({ action, location: nextLocation, delta });
          }
        },
        listen(fn) {
          listener = fn;
          return () => {
            listener = null;
          };
        }
      };
      return history;
    }
    function createBrowserHistory(options = {}) {
      function createBrowserLocation(window2, globalHistory) {
        let { pathname, search, hash } = window2.location;
        return createLocation(
          "",
          { pathname, search, hash },
          // state defaults to `null` because `window.history.state` does
          globalHistory.state && globalHistory.state.usr || null,
          globalHistory.state && globalHistory.state.key || "default"
        );
      }
      function createBrowserHref(window2, to) {
        return typeof to === "string" ? to : createPath(to);
      }
      return getUrlBasedHistory(
        createBrowserLocation,
        createBrowserHref,
        null,
        options
      );
    }
    function createHashHistory(options = {}) {
      function createHashLocation(window2, globalHistory) {
        let {
          pathname = "/",
          search = "",
          hash = ""
        } = parsePath(window2.location.hash.substring(1));
        if (!pathname.startsWith("/") && !pathname.startsWith(".")) {
          pathname = "/" + pathname;
        }
        return createLocation(
          "",
          { pathname, search, hash },
          // state defaults to `null` because `window.history.state` does
          globalHistory.state && globalHistory.state.usr || null,
          globalHistory.state && globalHistory.state.key || "default"
        );
      }
      function createHashHref(window2, to) {
        let base = window2.document.querySelector("base");
        let href2 = "";
        if (base && base.getAttribute("href")) {
          let url = window2.location.href;
          let hashIndex = url.indexOf("#");
          href2 = hashIndex === -1 ? url : url.slice(0, hashIndex);
        }
        return href2 + "#" + (typeof to === "string" ? to : createPath(to));
      }
      function validateHashLocation(location, to) {
        warning(
          location.pathname.charAt(0) === "/",
          `relative pathnames are not supported in hash history.push(${JSON.stringify(
            to
          )})`
        );
      }
      return getUrlBasedHistory(
        createHashLocation,
        createHashHref,
        validateHashLocation,
        options
      );
    }
    function invariant(value, message) {
      if (value === false || value === null || typeof value === "undefined") {
        throw new Error(message);
      }
    }
    function warning(cond, message) {
      if (!cond) {
        if (typeof console !== "undefined") console.warn(message);
        try {
          throw new Error(message);
        } catch (e) {
        }
      }
    }
    function createKey() {
      return Math.random().toString(36).substring(2, 10);
    }
    function getHistoryState(location, index) {
      return {
        usr: location.state,
        key: location.key,
        idx: index
      };
    }
    function createLocation(current, to, state = null, key) {
      let location = {
        pathname: typeof current === "string" ? current : current.pathname,
        search: "",
        hash: "",
        ...typeof to === "string" ? parsePath(to) : to,
        state,
        // TODO: This could be cleaned up.  push/replace should probably just take
        // full Locations now and avoid the need to run through this flow at all
        // But that's a pretty big refactor to the current test suite so going to
        // keep as is for the time being and just let any incoming keys take precedence
        key: to && to.key || key || createKey()
      };
      return location;
    }
    function createPath({
      pathname = "/",
      search = "",
      hash = ""
    }) {
      if (search && search !== "?")
        pathname += search.charAt(0) === "?" ? search : "?" + search;
      if (hash && hash !== "#")
        pathname += hash.charAt(0) === "#" ? hash : "#" + hash;
      return pathname;
    }
    function parsePath(path) {
      let parsedPath = {};
      if (path) {
        let hashIndex = path.indexOf("#");
        if (hashIndex >= 0) {
          parsedPath.hash = path.substring(hashIndex);
          path = path.substring(0, hashIndex);
        }
        let searchIndex = path.indexOf("?");
        if (searchIndex >= 0) {
          parsedPath.search = path.substring(searchIndex);
          path = path.substring(0, searchIndex);
        }
        if (path) {
          parsedPath.pathname = path;
        }
      }
      return parsedPath;
    }
    function getUrlBasedHistory(getLocation, createHref2, validateLocation, options = {}) {
      let { window: window2 = document.defaultView, v5Compat = false } = options;
      let globalHistory = window2.history;
      let action = "POP";
      let listener = null;
      let index = getIndex();
      if (index == null) {
        index = 0;
        globalHistory.replaceState({ ...globalHistory.state, idx: index }, "");
      }
      function getIndex() {
        let state = globalHistory.state || { idx: null };
        return state.idx;
      }
      function handlePop() {
        action = "POP";
        let nextIndex = getIndex();
        let delta = nextIndex == null ? null : nextIndex - index;
        index = nextIndex;
        if (listener) {
          listener({ action, location: history.location, delta });
        }
      }
      function push(to, state) {
        action = "PUSH";
        let location = createLocation(history.location, to, state);
        if (validateLocation) validateLocation(location, to);
        index = getIndex() + 1;
        let historyState = getHistoryState(location, index);
        let url = history.createHref(location);
        try {
          globalHistory.pushState(historyState, "", url);
        } catch (error) {
          if (error instanceof DOMException && error.name === "DataCloneError") {
            throw error;
          }
          window2.location.assign(url);
        }
        if (v5Compat && listener) {
          listener({ action, location: history.location, delta: 1 });
        }
      }
      function replace2(to, state) {
        action = "REPLACE";
        let location = createLocation(history.location, to, state);
        if (validateLocation) validateLocation(location, to);
        index = getIndex();
        let historyState = getHistoryState(location, index);
        let url = history.createHref(location);
        globalHistory.replaceState(historyState, "", url);
        if (v5Compat && listener) {
          listener({ action, location: history.location, delta: 0 });
        }
      }
      function createURL(to) {
        let base = window2.location.origin !== "null" ? window2.location.origin : window2.location.href;
        let href2 = typeof to === "string" ? to : createPath(to);
        href2 = href2.replace(/ $/, "%20");
        invariant(
          base,
          `No window.location.(origin|href) available to create URL for href: ${href2}`
        );
        return new URL(href2, base);
      }
      let history = {
        get action() {
          return action;
        },
        get location() {
          return getLocation(window2, globalHistory);
        },
        listen(fn) {
          if (listener) {
            throw new Error("A history only accepts one active listener");
          }
          window2.addEventListener(PopStateEventType, handlePop);
          listener = fn;
          return () => {
            window2.removeEventListener(PopStateEventType, handlePop);
            listener = null;
          };
        },
        createHref(to) {
          return createHref2(window2, to);
        },
        createURL,
        encodeLocation(to) {
          let url = createURL(to);
          return {
            pathname: url.pathname,
            search: url.search,
            hash: url.hash
          };
        },
        push,
        replace: replace2,
        go(n) {
          return globalHistory.go(n);
        }
      };
      return history;
    }
    var immutableRouteKeys = /* @__PURE__ */ new Set([
      "lazy",
      "caseSensitive",
      "path",
      "id",
      "index",
      "children"
    ]);
    function isIndexRoute(route) {
      return route.index === true;
    }
    function convertRoutesToDataRoutes(routes, mapRouteProperties2, parentPath = [], manifest = {}) {
      return routes.map((route, index) => {
        let treePath = [...parentPath, String(index)];
        let id = typeof route.id === "string" ? route.id : treePath.join("-");
        invariant(
          route.index !== true || !route.children,
          `Cannot specify children on an index route`
        );
        invariant(
          !manifest[id],
          `Found a route id collision on id "${id}".  Route id's must be globally unique within Data Router usages`
        );
        if (isIndexRoute(route)) {
          let indexRoute = {
            ...route,
            ...mapRouteProperties2(route),
            id
          };
          manifest[id] = indexRoute;
          return indexRoute;
        } else {
          let pathOrLayoutRoute = {
            ...route,
            ...mapRouteProperties2(route),
            id,
            children: void 0
          };
          manifest[id] = pathOrLayoutRoute;
          if (route.children) {
            pathOrLayoutRoute.children = convertRoutesToDataRoutes(
              route.children,
              mapRouteProperties2,
              treePath,
              manifest
            );
          }
          return pathOrLayoutRoute;
        }
      });
    }
    function matchRoutes(routes, locationArg, basename = "/") {
      return matchRoutesImpl(routes, locationArg, basename, false);
    }
    function matchRoutesImpl(routes, locationArg, basename, allowPartial) {
      let location = typeof locationArg === "string" ? parsePath(locationArg) : locationArg;
      let pathname = stripBasename(location.pathname || "/", basename);
      if (pathname == null) {
        return null;
      }
      let branches = flattenRoutes(routes);
      rankRouteBranches(branches);
      let matches = null;
      for (let i = 0; matches == null && i < branches.length; ++i) {
        let decoded = decodePath(pathname);
        matches = matchRouteBranch(
          branches[i],
          decoded,
          allowPartial
        );
      }
      return matches;
    }
    function convertRouteMatchToUiMatch(match, loaderData) {
      let { route, pathname, params } = match;
      return {
        id: route.id,
        pathname,
        params,
        data: loaderData[route.id],
        handle: route.handle
      };
    }
    function flattenRoutes(routes, branches = [], parentsMeta = [], parentPath = "") {
      let flattenRoute = (route, index, relativePath) => {
        let meta = {
          relativePath: relativePath === void 0 ? route.path || "" : relativePath,
          caseSensitive: route.caseSensitive === true,
          childrenIndex: index,
          route
        };
        if (meta.relativePath.startsWith("/")) {
          invariant(
            meta.relativePath.startsWith(parentPath),
            `Absolute route path "${meta.relativePath}" nested under path "${parentPath}" is not valid. An absolute child route path must start with the combined path of all its parent routes.`
          );
          meta.relativePath = meta.relativePath.slice(parentPath.length);
        }
        let path = joinPaths([parentPath, meta.relativePath]);
        let routesMeta = parentsMeta.concat(meta);
        if (route.children && route.children.length > 0) {
          invariant(
            // Our types know better, but runtime JS may not!
            // @ts-expect-error
            route.index !== true,
            `Index routes must not have child routes. Please remove all child routes from route path "${path}".`
          );
          flattenRoutes(route.children, branches, routesMeta, path);
        }
        if (route.path == null && !route.index) {
          return;
        }
        branches.push({
          path,
          score: computeScore(path, route.index),
          routesMeta
        });
      };
      routes.forEach((route, index) => {
        var _a;
        if (route.path === "" || !((_a = route.path) == null ? void 0 : _a.includes("?"))) {
          flattenRoute(route, index);
        } else {
          for (let exploded of explodeOptionalSegments(route.path)) {
            flattenRoute(route, index, exploded);
          }
        }
      });
      return branches;
    }
    function explodeOptionalSegments(path) {
      let segments = path.split("/");
      if (segments.length === 0) return [];
      let [first, ...rest] = segments;
      let isOptional = first.endsWith("?");
      let required = first.replace(/\?$/, "");
      if (rest.length === 0) {
        return isOptional ? [required, ""] : [required];
      }
      let restExploded = explodeOptionalSegments(rest.join("/"));
      let result = [];
      result.push(
        ...restExploded.map(
          (subpath) => subpath === "" ? required : [required, subpath].join("/")
        )
      );
      if (isOptional) {
        result.push(...restExploded);
      }
      return result.map(
        (exploded) => path.startsWith("/") && exploded === "" ? "/" : exploded
      );
    }
    function rankRouteBranches(branches) {
      branches.sort(
        (a, b) => a.score !== b.score ? b.score - a.score : compareIndexes(
          a.routesMeta.map((meta) => meta.childrenIndex),
          b.routesMeta.map((meta) => meta.childrenIndex)
        )
      );
    }
    var paramRe = /^:[\w-]+$/;
    var dynamicSegmentValue = 3;
    var indexRouteValue = 2;
    var emptySegmentValue = 1;
    var staticSegmentValue = 10;
    var splatPenalty = -2;
    var isSplat = (s) => s === "*";
    function computeScore(path, index) {
      let segments = path.split("/");
      let initialScore = segments.length;
      if (segments.some(isSplat)) {
        initialScore += splatPenalty;
      }
      if (index) {
        initialScore += indexRouteValue;
      }
      return segments.filter((s) => !isSplat(s)).reduce(
        (score, segment) => score + (paramRe.test(segment) ? dynamicSegmentValue : segment === "" ? emptySegmentValue : staticSegmentValue),
        initialScore
      );
    }
    function compareIndexes(a, b) {
      let siblings = a.length === b.length && a.slice(0, -1).every((n, i) => n === b[i]);
      return siblings ? (
        // If two routes are siblings, we should try to match the earlier sibling
        // first. This allows people to have fine-grained control over the matching
        // behavior by simply putting routes with identical paths in the order they
        // want them tried.
        a[a.length - 1] - b[b.length - 1]
      ) : (
        // Otherwise, it doesn't really make sense to rank non-siblings by index,
        // so they sort equally.
        0
      );
    }
    function matchRouteBranch(branch, pathname, allowPartial = false) {
      let { routesMeta } = branch;
      let matchedParams = {};
      let matchedPathname = "/";
      let matches = [];
      for (let i = 0; i < routesMeta.length; ++i) {
        let meta = routesMeta[i];
        let end = i === routesMeta.length - 1;
        let remainingPathname = matchedPathname === "/" ? pathname : pathname.slice(matchedPathname.length) || "/";
        let match = matchPath(
          { path: meta.relativePath, caseSensitive: meta.caseSensitive, end },
          remainingPathname
        );
        let route = meta.route;
        if (!match && end && allowPartial && !routesMeta[routesMeta.length - 1].route.index) {
          match = matchPath(
            {
              path: meta.relativePath,
              caseSensitive: meta.caseSensitive,
              end: false
            },
            remainingPathname
          );
        }
        if (!match) {
          return null;
        }
        Object.assign(matchedParams, match.params);
        matches.push({
          // TODO: Can this as be avoided?
          params: matchedParams,
          pathname: joinPaths([matchedPathname, match.pathname]),
          pathnameBase: normalizePathname(
            joinPaths([matchedPathname, match.pathnameBase])
          ),
          route
        });
        if (match.pathnameBase !== "/") {
          matchedPathname = joinPaths([matchedPathname, match.pathnameBase]);
        }
      }
      return matches;
    }
    function generatePath(originalPath, params = {}) {
      let path = originalPath;
      if (path.endsWith("*") && path !== "*" && !path.endsWith("/*")) {
        warning(
          false,
          `Route path "${path}" will be treated as if it were "${path.replace(/\*$/, "/*")}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${path.replace(/\*$/, "/*")}".`
        );
        path = path.replace(/\*$/, "/*");
      }
      const prefix = path.startsWith("/") ? "/" : "";
      const stringify = (p) => p == null ? "" : typeof p === "string" ? p : String(p);
      const segments = path.split(/\/+/).map((segment, index, array) => {
        const isLastSegment = index === array.length - 1;
        if (isLastSegment && segment === "*") {
          const star = "*";
          return stringify(params[star]);
        }
        const keyMatch = segment.match(/^:([\w-]+)(\??)$/);
        if (keyMatch) {
          const [, key, optional] = keyMatch;
          let param = params[key];
          invariant(optional === "?" || param != null, `Missing ":${key}" param`);
          return stringify(param);
        }
        return segment.replace(/\?$/g, "");
      }).filter((segment) => !!segment);
      return prefix + segments.join("/");
    }
    function matchPath(pattern, pathname) {
      if (typeof pattern === "string") {
        pattern = { path: pattern, caseSensitive: false, end: true };
      }
      let [matcher, compiledParams] = compilePath(
        pattern.path,
        pattern.caseSensitive,
        pattern.end
      );
      let match = pathname.match(matcher);
      if (!match) return null;
      let matchedPathname = match[0];
      let pathnameBase = matchedPathname.replace(/(.)\/+$/, "$1");
      let captureGroups = match.slice(1);
      let params = compiledParams.reduce(
        (memo2, { paramName, isOptional }, index) => {
          if (paramName === "*") {
            let splatValue = captureGroups[index] || "";
            pathnameBase = matchedPathname.slice(0, matchedPathname.length - splatValue.length).replace(/(.)\/+$/, "$1");
          }
          const value = captureGroups[index];
          if (isOptional && !value) {
            memo2[paramName] = void 0;
          } else {
            memo2[paramName] = (value || "").replace(/%2F/g, "/");
          }
          return memo2;
        },
        {}
      );
      return {
        params,
        pathname: matchedPathname,
        pathnameBase,
        pattern
      };
    }
    function compilePath(path, caseSensitive = false, end = true) {
      warning(
        path === "*" || !path.endsWith("*") || path.endsWith("/*"),
        `Route path "${path}" will be treated as if it were "${path.replace(/\*$/, "/*")}" because the \`*\` character must always follow a \`/\` in the pattern. To get rid of this warning, please change the route path to "${path.replace(/\*$/, "/*")}".`
      );
      let params = [];
      let regexpSource = "^" + path.replace(/\/*\*?$/, "").replace(/^\/*/, "/").replace(/[\\.*+^${}|()[\]]/g, "\\$&").replace(
        /\/:([\w-]+)(\?)?/g,
        (_, paramName, isOptional) => {
          params.push({ paramName, isOptional: isOptional != null });
          return isOptional ? "/?([^\\/]+)?" : "/([^\\/]+)";
        }
      );
      if (path.endsWith("*")) {
        params.push({ paramName: "*" });
        regexpSource += path === "*" || path === "/*" ? "(.*)$" : "(?:\\/(.+)|\\/*)$";
      } else if (end) {
        regexpSource += "\\/*$";
      } else if (path !== "" && path !== "/") {
        regexpSource += "(?:(?=\\/|$))";
      } else {
      }
      let matcher = new RegExp(regexpSource, caseSensitive ? void 0 : "i");
      return [matcher, params];
    }
    function decodePath(value) {
      try {
        return value.split("/").map((v) => decodeURIComponent(v).replace(/\//g, "%2F")).join("/");
      } catch (error) {
        warning(
          false,
          `The URL path "${value}" could not be decoded because it is a malformed URL segment. This is probably due to a bad percent encoding (${error}).`
        );
        return value;
      }
    }
    function stripBasename(pathname, basename) {
      if (basename === "/") return pathname;
      if (!pathname.toLowerCase().startsWith(basename.toLowerCase())) {
        return null;
      }
      let startIndex = basename.endsWith("/") ? basename.length - 1 : basename.length;
      let nextChar = pathname.charAt(startIndex);
      if (nextChar && nextChar !== "/") {
        return null;
      }
      return pathname.slice(startIndex) || "/";
    }
    function resolvePath(to, fromPathname = "/") {
      let {
        pathname: toPathname,
        search = "",
        hash = ""
      } = typeof to === "string" ? parsePath(to) : to;
      let pathname = toPathname ? toPathname.startsWith("/") ? toPathname : resolvePathname(toPathname, fromPathname) : fromPathname;
      return {
        pathname,
        search: normalizeSearch(search),
        hash: normalizeHash(hash)
      };
    }
    function resolvePathname(relativePath, fromPathname) {
      let segments = fromPathname.replace(/\/+$/, "").split("/");
      let relativeSegments = relativePath.split("/");
      relativeSegments.forEach((segment) => {
        if (segment === "..") {
          if (segments.length > 1) segments.pop();
        } else if (segment !== ".") {
          segments.push(segment);
        }
      });
      return segments.length > 1 ? segments.join("/") : "/";
    }
    function getInvalidPathError(char, field, dest, path) {
      return `Cannot include a '${char}' character in a manually specified \`to.${field}\` field [${JSON.stringify(
        path
      )}].  Please separate it out to the \`to.${dest}\` field. Alternatively you may provide the full path as a string in <Link to="..."> and the router will parse it for you.`;
    }
    function getPathContributingMatches(matches) {
      return matches.filter(
        (match, index) => index === 0 || match.route.path && match.route.path.length > 0
      );
    }
    function getResolveToMatches(matches) {
      let pathMatches = getPathContributingMatches(matches);
      return pathMatches.map(
        (match, idx) => idx === pathMatches.length - 1 ? match.pathname : match.pathnameBase
      );
    }
    function resolveTo(toArg, routePathnames, locationPathname, isPathRelative = false) {
      let to;
      if (typeof toArg === "string") {
        to = parsePath(toArg);
      } else {
        to = { ...toArg };
        invariant(
          !to.pathname || !to.pathname.includes("?"),
          getInvalidPathError("?", "pathname", "search", to)
        );
        invariant(
          !to.pathname || !to.pathname.includes("#"),
          getInvalidPathError("#", "pathname", "hash", to)
        );
        invariant(
          !to.search || !to.search.includes("#"),
          getInvalidPathError("#", "search", "hash", to)
        );
      }
      let isEmptyPath = toArg === "" || to.pathname === "";
      let toPathname = isEmptyPath ? "/" : to.pathname;
      let from;
      if (toPathname == null) {
        from = locationPathname;
      } else {
        let routePathnameIndex = routePathnames.length - 1;
        if (!isPathRelative && toPathname.startsWith("..")) {
          let toSegments = toPathname.split("/");
          while (toSegments[0] === "..") {
            toSegments.shift();
            routePathnameIndex -= 1;
          }
          to.pathname = toSegments.join("/");
        }
        from = routePathnameIndex >= 0 ? routePathnames[routePathnameIndex] : "/";
      }
      let path = resolvePath(to, from);
      let hasExplicitTrailingSlash = toPathname && toPathname !== "/" && toPathname.endsWith("/");
      let hasCurrentTrailingSlash = (isEmptyPath || toPathname === ".") && locationPathname.endsWith("/");
      if (!path.pathname.endsWith("/") && (hasExplicitTrailingSlash || hasCurrentTrailingSlash)) {
        path.pathname += "/";
      }
      return path;
    }
    var joinPaths = (paths) => paths.join("/").replace(/\/\/+/g, "/");
    var normalizePathname = (pathname) => pathname.replace(/\/+$/, "").replace(/^\/*/, "/");
    var normalizeSearch = (search) => !search || search === "?" ? "" : search.startsWith("?") ? search : "?" + search;
    var normalizeHash = (hash) => !hash || hash === "#" ? "" : hash.startsWith("#") ? hash : "#" + hash;
    var DataWithResponseInit = class {
      constructor(data2, init) {
        this.type = "DataWithResponseInit";
        this.data = data2;
        this.init = init || null;
      }
    };
    function data(data2, init) {
      return new DataWithResponseInit(
        data2,
        typeof init === "number" ? { status: init } : init
      );
    }
    var redirect = (url, init = 302) => {
      let responseInit = init;
      if (typeof responseInit === "number") {
        responseInit = { status: responseInit };
      } else if (typeof responseInit.status === "undefined") {
        responseInit.status = 302;
      }
      let headers = new Headers(responseInit.headers);
      headers.set("Location", url);
      return new Response(null, { ...responseInit, headers });
    };
    var redirectDocument = (url, init) => {
      let response = redirect(url, init);
      response.headers.set("X-Remix-Reload-Document", "true");
      return response;
    };
    var replace = (url, init) => {
      let response = redirect(url, init);
      response.headers.set("X-Remix-Replace", "true");
      return response;
    };
    var ErrorResponseImpl = class {
      constructor(status, statusText, data2, internal = false) {
        this.status = status;
        this.statusText = statusText || "";
        this.internal = internal;
        if (data2 instanceof Error) {
          this.data = data2.toString();
          this.error = data2;
        } else {
          this.data = data2;
        }
      }
    };
    function isRouteErrorResponse(error) {
      return error != null && typeof error.status === "number" && typeof error.statusText === "string" && typeof error.internal === "boolean" && "data" in error;
    }
    var validMutationMethodsArr = [
      "POST",
      "PUT",
      "PATCH",
      "DELETE"
    ];
    var validMutationMethods = new Set(
      validMutationMethodsArr
    );
    var validRequestMethodsArr = [
      "GET",
      ...validMutationMethodsArr
    ];
    var validRequestMethods = new Set(validRequestMethodsArr);
    var redirectStatusCodes = /* @__PURE__ */ new Set([301, 302, 303, 307, 308]);
    var redirectPreserveMethodStatusCodes = /* @__PURE__ */ new Set([307, 308]);
    var IDLE_NAVIGATION = {
      state: "idle",
      location: void 0,
      formMethod: void 0,
      formAction: void 0,
      formEncType: void 0,
      formData: void 0,
      json: void 0,
      text: void 0
    };
    var IDLE_FETCHER = {
      state: "idle",
      data: void 0,
      formMethod: void 0,
      formAction: void 0,
      formEncType: void 0,
      formData: void 0,
      json: void 0,
      text: void 0
    };
    var IDLE_BLOCKER = {
      state: "unblocked",
      proceed: void 0,
      reset: void 0,
      location: void 0
    };
    var ABSOLUTE_URL_REGEX = /^(?:[a-z][a-z0-9+.-]*:|\/\/)/i;
    var defaultMapRouteProperties = (route) => ({
      hasErrorBoundary: Boolean(route.hasErrorBoundary)
    });
    var TRANSITIONS_STORAGE_KEY = "remix-router-transitions";
    var ResetLoaderDataSymbol = Symbol("ResetLoaderData");
    function createRouter(init) {
      const routerWindow = init.window ? init.window : typeof window !== "undefined" ? window : void 0;
      const isBrowser2 = typeof routerWindow !== "undefined" && typeof routerWindow.document !== "undefined" && typeof routerWindow.document.createElement !== "undefined";
      invariant(
        init.routes.length > 0,
        "You must provide a non-empty routes array to createRouter"
      );
      let mapRouteProperties2 = init.mapRouteProperties || defaultMapRouteProperties;
      let manifest = {};
      let dataRoutes = convertRoutesToDataRoutes(
        init.routes,
        mapRouteProperties2,
        void 0,
        manifest
      );
      let inFlightDataRoutes;
      let basename = init.basename || "/";
      let dataStrategyImpl = init.dataStrategy || defaultDataStrategy;
      let patchRoutesOnNavigationImpl = init.patchRoutesOnNavigation;
      let future = {
        ...init.future
      };
      let unlistenHistory = null;
      let subscribers = /* @__PURE__ */ new Set();
      let savedScrollPositions2 = null;
      let getScrollRestorationKey2 = null;
      let getScrollPosition = null;
      let initialScrollRestored = init.hydrationData != null;
      let initialMatches = matchRoutes(dataRoutes, init.history.location, basename);
      let initialMatchesIsFOW = false;
      let initialErrors = null;
      if (initialMatches == null && !patchRoutesOnNavigationImpl) {
        let error = getInternalRouterError(404, {
          pathname: init.history.location.pathname
        });
        let { matches, route } = getShortCircuitMatches(dataRoutes);
        initialMatches = matches;
        initialErrors = { [route.id]: error };
      }
      if (initialMatches && !init.hydrationData) {
        let fogOfWar = checkFogOfWar(
          initialMatches,
          dataRoutes,
          init.history.location.pathname
        );
        if (fogOfWar.active) {
          initialMatches = null;
        }
      }
      let initialized;
      if (!initialMatches) {
        initialized = false;
        initialMatches = [];
        let fogOfWar = checkFogOfWar(
          null,
          dataRoutes,
          init.history.location.pathname
        );
        if (fogOfWar.active && fogOfWar.matches) {
          initialMatchesIsFOW = true;
          initialMatches = fogOfWar.matches;
        }
      } else if (initialMatches.some((m) => m.route.lazy)) {
        initialized = false;
      } else if (!initialMatches.some((m) => m.route.loader)) {
        initialized = true;
      } else {
        let loaderData = init.hydrationData ? init.hydrationData.loaderData : null;
        let errors = init.hydrationData ? init.hydrationData.errors : null;
        if (errors) {
          let idx = initialMatches.findIndex(
            (m) => errors[m.route.id] !== void 0
          );
          initialized = initialMatches.slice(0, idx + 1).every((m) => !shouldLoadRouteOnHydration(m.route, loaderData, errors));
        } else {
          initialized = initialMatches.every(
            (m) => !shouldLoadRouteOnHydration(m.route, loaderData, errors)
          );
        }
      }
      let router;
      let state = {
        historyAction: init.history.action,
        location: init.history.location,
        matches: initialMatches,
        initialized,
        navigation: IDLE_NAVIGATION,
        // Don't restore on initial updateState() if we were SSR'd
        restoreScrollPosition: init.hydrationData != null ? false : null,
        preventScrollReset: false,
        revalidation: "idle",
        loaderData: init.hydrationData && init.hydrationData.loaderData || {},
        actionData: init.hydrationData && init.hydrationData.actionData || null,
        errors: init.hydrationData && init.hydrationData.errors || initialErrors,
        fetchers: /* @__PURE__ */ new Map(),
        blockers: /* @__PURE__ */ new Map()
      };
      let pendingAction = "POP";
      let pendingPreventScrollReset = false;
      let pendingNavigationController;
      let pendingViewTransitionEnabled = false;
      let appliedViewTransitions = /* @__PURE__ */ new Map();
      let removePageHideEventListener = null;
      let isUninterruptedRevalidation = false;
      let isRevalidationRequired = false;
      let cancelledFetcherLoads = /* @__PURE__ */ new Set();
      let fetchControllers = /* @__PURE__ */ new Map();
      let incrementingLoadId = 0;
      let pendingNavigationLoadId = -1;
      let fetchReloadIds = /* @__PURE__ */ new Map();
      let fetchRedirectIds = /* @__PURE__ */ new Set();
      let fetchLoadMatches = /* @__PURE__ */ new Map();
      let activeFetchers = /* @__PURE__ */ new Map();
      let fetchersQueuedForDeletion = /* @__PURE__ */ new Set();
      let blockerFunctions = /* @__PURE__ */ new Map();
      let unblockBlockerHistoryUpdate = void 0;
      let pendingRevalidationDfd = null;
      function initialize() {
        unlistenHistory = init.history.listen(
          ({ action: historyAction, location, delta }) => {
            if (unblockBlockerHistoryUpdate) {
              unblockBlockerHistoryUpdate();
              unblockBlockerHistoryUpdate = void 0;
              return;
            }
            warning(
              blockerFunctions.size === 0 || delta != null,
              "You are trying to use a blocker on a POP navigation to a location that was not created by @remix-run/router. This will fail silently in production. This can happen if you are navigating outside the router via `window.history.pushState`/`window.location.hash` instead of using router navigation APIs.  This can also happen if you are using createHashRouter and the user manually changes the URL."
            );
            let blockerKey = shouldBlockNavigation({
              currentLocation: state.location,
              nextLocation: location,
              historyAction
            });
            if (blockerKey && delta != null) {
              let nextHistoryUpdatePromise = new Promise((resolve) => {
                unblockBlockerHistoryUpdate = resolve;
              });
              init.history.go(delta * -1);
              updateBlocker(blockerKey, {
                state: "blocked",
                location,
                proceed() {
                  updateBlocker(blockerKey, {
                    state: "proceeding",
                    proceed: void 0,
                    reset: void 0,
                    location
                  });
                  nextHistoryUpdatePromise.then(() => init.history.go(delta));
                },
                reset() {
                  let blockers = new Map(state.blockers);
                  blockers.set(blockerKey, IDLE_BLOCKER);
                  updateState({ blockers });
                }
              });
              return;
            }
            return startNavigation(historyAction, location);
          }
        );
        if (isBrowser2) {
          restoreAppliedTransitions(routerWindow, appliedViewTransitions);
          let _saveAppliedTransitions = () => persistAppliedTransitions(routerWindow, appliedViewTransitions);
          routerWindow.addEventListener("pagehide", _saveAppliedTransitions);
          removePageHideEventListener = () => routerWindow.removeEventListener("pagehide", _saveAppliedTransitions);
        }
        if (!state.initialized) {
          startNavigation("POP", state.location, {
            initialHydration: true
          });
        }
        return router;
      }
      function dispose() {
        if (unlistenHistory) {
          unlistenHistory();
        }
        if (removePageHideEventListener) {
          removePageHideEventListener();
        }
        subscribers.clear();
        pendingNavigationController && pendingNavigationController.abort();
        state.fetchers.forEach((_, key) => deleteFetcher(key));
        state.blockers.forEach((_, key) => deleteBlocker(key));
      }
      function subscribe(fn) {
        subscribers.add(fn);
        return () => subscribers.delete(fn);
      }
      function updateState(newState, opts = {}) {
        state = {
          ...state,
          ...newState
        };
        let unmountedFetchers = [];
        let mountedFetchers = [];
        state.fetchers.forEach((fetcher, key) => {
          if (fetcher.state === "idle") {
            if (fetchersQueuedForDeletion.has(key)) {
              unmountedFetchers.push(key);
            } else {
              mountedFetchers.push(key);
            }
          }
        });
        fetchersQueuedForDeletion.forEach((key) => {
          if (!state.fetchers.has(key) && !fetchControllers.has(key)) {
            unmountedFetchers.push(key);
          }
        });
        [...subscribers].forEach(
          (subscriber) => subscriber(state, {
            deletedFetchers: unmountedFetchers,
            viewTransitionOpts: opts.viewTransitionOpts,
            flushSync: opts.flushSync === true
          })
        );
        unmountedFetchers.forEach((key) => deleteFetcher(key));
        mountedFetchers.forEach((key) => state.fetchers.delete(key));
      }
      function completeNavigation(location, newState, { flushSync } = {}) {
        var _a, _b;
        let isActionReload = state.actionData != null && state.navigation.formMethod != null && isMutationMethod(state.navigation.formMethod) && state.navigation.state === "loading" && ((_a = location.state) == null ? void 0 : _a._isRedirect) !== true;
        let actionData;
        if (newState.actionData) {
          if (Object.keys(newState.actionData).length > 0) {
            actionData = newState.actionData;
          } else {
            actionData = null;
          }
        } else if (isActionReload) {
          actionData = state.actionData;
        } else {
          actionData = null;
        }
        let loaderData = newState.loaderData ? mergeLoaderData(
          state.loaderData,
          newState.loaderData,
          newState.matches || [],
          newState.errors
        ) : state.loaderData;
        let blockers = state.blockers;
        if (blockers.size > 0) {
          blockers = new Map(blockers);
          blockers.forEach((_, k) => blockers.set(k, IDLE_BLOCKER));
        }
        let preventScrollReset = pendingPreventScrollReset === true || state.navigation.formMethod != null && isMutationMethod(state.navigation.formMethod) && ((_b = location.state) == null ? void 0 : _b._isRedirect) !== true;
        if (inFlightDataRoutes) {
          dataRoutes = inFlightDataRoutes;
          inFlightDataRoutes = void 0;
        }
        if (isUninterruptedRevalidation) {
        } else if (pendingAction === "POP") {
        } else if (pendingAction === "PUSH") {
          init.history.push(location, location.state);
        } else if (pendingAction === "REPLACE") {
          init.history.replace(location, location.state);
        }
        let viewTransitionOpts;
        if (pendingAction === "POP") {
          let priorPaths = appliedViewTransitions.get(state.location.pathname);
          if (priorPaths && priorPaths.has(location.pathname)) {
            viewTransitionOpts = {
              currentLocation: state.location,
              nextLocation: location
            };
          } else if (appliedViewTransitions.has(location.pathname)) {
            viewTransitionOpts = {
              currentLocation: location,
              nextLocation: state.location
            };
          }
        } else if (pendingViewTransitionEnabled) {
          let toPaths = appliedViewTransitions.get(state.location.pathname);
          if (toPaths) {
            toPaths.add(location.pathname);
          } else {
            toPaths = /* @__PURE__ */ new Set([location.pathname]);
            appliedViewTransitions.set(state.location.pathname, toPaths);
          }
          viewTransitionOpts = {
            currentLocation: state.location,
            nextLocation: location
          };
        }
        updateState(
          {
            ...newState,
            // matches, errors, fetchers go through as-is
            actionData,
            loaderData,
            historyAction: pendingAction,
            location,
            initialized: true,
            navigation: IDLE_NAVIGATION,
            revalidation: "idle",
            restoreScrollPosition: getSavedScrollPosition(
              location,
              newState.matches || state.matches
            ),
            preventScrollReset,
            blockers
          },
          {
            viewTransitionOpts,
            flushSync: flushSync === true
          }
        );
        pendingAction = "POP";
        pendingPreventScrollReset = false;
        pendingViewTransitionEnabled = false;
        isUninterruptedRevalidation = false;
        isRevalidationRequired = false;
        pendingRevalidationDfd == null ? void 0 : pendingRevalidationDfd.resolve();
        pendingRevalidationDfd = null;
      }
      async function navigate(to, opts) {
        if (typeof to === "number") {
          init.history.go(to);
          return;
        }
        let normalizedPath = normalizeTo(
          state.location,
          state.matches,
          basename,
          to,
          opts == null ? void 0 : opts.fromRouteId,
          opts == null ? void 0 : opts.relative
        );
        let { path, submission, error } = normalizeNavigateOptions(
          false,
          normalizedPath,
          opts
        );
        let currentLocation = state.location;
        let nextLocation = createLocation(state.location, path, opts && opts.state);
        nextLocation = {
          ...nextLocation,
          ...init.history.encodeLocation(nextLocation)
        };
        let userReplace = opts && opts.replace != null ? opts.replace : void 0;
        let historyAction = "PUSH";
        if (userReplace === true) {
          historyAction = "REPLACE";
        } else if (userReplace === false) {
        } else if (submission != null && isMutationMethod(submission.formMethod) && submission.formAction === state.location.pathname + state.location.search) {
          historyAction = "REPLACE";
        }
        let preventScrollReset = opts && "preventScrollReset" in opts ? opts.preventScrollReset === true : void 0;
        let flushSync = (opts && opts.flushSync) === true;
        let blockerKey = shouldBlockNavigation({
          currentLocation,
          nextLocation,
          historyAction
        });
        if (blockerKey) {
          updateBlocker(blockerKey, {
            state: "blocked",
            location: nextLocation,
            proceed() {
              updateBlocker(blockerKey, {
                state: "proceeding",
                proceed: void 0,
                reset: void 0,
                location: nextLocation
              });
              navigate(to, opts);
            },
            reset() {
              let blockers = new Map(state.blockers);
              blockers.set(blockerKey, IDLE_BLOCKER);
              updateState({ blockers });
            }
          });
          return;
        }
        await startNavigation(historyAction, nextLocation, {
          submission,
          // Send through the formData serialization error if we have one so we can
          // render at the right error boundary after we match routes
          pendingError: error,
          preventScrollReset,
          replace: opts && opts.replace,
          enableViewTransition: opts && opts.viewTransition,
          flushSync
        });
      }
      function revalidate() {
        if (!pendingRevalidationDfd) {
          pendingRevalidationDfd = createDeferred();
        }
        interruptActiveLoads();
        updateState({ revalidation: "loading" });
        let promise = pendingRevalidationDfd.promise;
        if (state.navigation.state === "submitting") {
          return promise;
        }
        if (state.navigation.state === "idle") {
          startNavigation(state.historyAction, state.location, {
            startUninterruptedRevalidation: true
          });
          return promise;
        }
        startNavigation(
          pendingAction || state.historyAction,
          state.navigation.location,
          {
            overrideNavigation: state.navigation,
            // Proxy through any rending view transition
            enableViewTransition: pendingViewTransitionEnabled === true
          }
        );
        return promise;
      }
      async function startNavigation(historyAction, location, opts) {
        pendingNavigationController && pendingNavigationController.abort();
        pendingNavigationController = null;
        pendingAction = historyAction;
        isUninterruptedRevalidation = (opts && opts.startUninterruptedRevalidation) === true;
        saveScrollPosition(state.location, state.matches);
        pendingPreventScrollReset = (opts && opts.preventScrollReset) === true;
        pendingViewTransitionEnabled = (opts && opts.enableViewTransition) === true;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let loadingNavigation = opts && opts.overrideNavigation;
        let matches = (opts == null ? void 0 : opts.initialHydration) && state.matches && state.matches.length > 0 && !initialMatchesIsFOW ? (
          // `matchRoutes()` has already been called if we're in here via `router.initialize()`
          state.matches
        ) : matchRoutes(routesToUse, location, basename);
        let flushSync = (opts && opts.flushSync) === true;
        if (matches && state.initialized && !isRevalidationRequired && isHashChangeOnly(state.location, location) && !(opts && opts.submission && isMutationMethod(opts.submission.formMethod))) {
          completeNavigation(location, { matches }, { flushSync });
          return;
        }
        let fogOfWar = checkFogOfWar(matches, routesToUse, location.pathname);
        if (fogOfWar.active && fogOfWar.matches) {
          matches = fogOfWar.matches;
        }
        if (!matches) {
          let { error, notFoundMatches, route } = handleNavigational404(
            location.pathname
          );
          completeNavigation(
            location,
            {
              matches: notFoundMatches,
              loaderData: {},
              errors: {
                [route.id]: error
              }
            },
            { flushSync }
          );
          return;
        }
        pendingNavigationController = new AbortController();
        let request = createClientSideRequest(
          init.history,
          location,
          pendingNavigationController.signal,
          opts && opts.submission
        );
        let pendingActionResult;
        if (opts && opts.pendingError) {
          pendingActionResult = [
            findNearestBoundary(matches).route.id,
            { type: "error", error: opts.pendingError }
          ];
        } else if (opts && opts.submission && isMutationMethod(opts.submission.formMethod)) {
          let actionResult = await handleAction(
            request,
            location,
            opts.submission,
            matches,
            fogOfWar.active,
            { replace: opts.replace, flushSync }
          );
          if (actionResult.shortCircuited) {
            return;
          }
          if (actionResult.pendingActionResult) {
            let [routeId, result] = actionResult.pendingActionResult;
            if (isErrorResult(result) && isRouteErrorResponse(result.error) && result.error.status === 404) {
              pendingNavigationController = null;
              completeNavigation(location, {
                matches: actionResult.matches,
                loaderData: {},
                errors: {
                  [routeId]: result.error
                }
              });
              return;
            }
          }
          matches = actionResult.matches || matches;
          pendingActionResult = actionResult.pendingActionResult;
          loadingNavigation = getLoadingNavigation(location, opts.submission);
          flushSync = false;
          fogOfWar.active = false;
          request = createClientSideRequest(
            init.history,
            request.url,
            request.signal
          );
        }
        let {
          shortCircuited,
          matches: updatedMatches,
          loaderData,
          errors
        } = await handleLoaders(
          request,
          location,
          matches,
          fogOfWar.active,
          loadingNavigation,
          opts && opts.submission,
          opts && opts.fetcherSubmission,
          opts && opts.replace,
          opts && opts.initialHydration === true,
          flushSync,
          pendingActionResult
        );
        if (shortCircuited) {
          return;
        }
        pendingNavigationController = null;
        completeNavigation(location, {
          matches: updatedMatches || matches,
          ...getActionDataForCommit(pendingActionResult),
          loaderData,
          errors
        });
      }
      async function handleAction(request, location, submission, matches, isFogOfWar, opts = {}) {
        interruptActiveLoads();
        let navigation = getSubmittingNavigation(location, submission);
        updateState({ navigation }, { flushSync: opts.flushSync === true });
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            matches,
            location.pathname,
            request.signal
          );
          if (discoverResult.type === "aborted") {
            return { shortCircuited: true };
          } else if (discoverResult.type === "error") {
            let boundaryId = findNearestBoundary(discoverResult.partialMatches).route.id;
            return {
              matches: discoverResult.partialMatches,
              pendingActionResult: [
                boundaryId,
                {
                  type: "error",
                  error: discoverResult.error
                }
              ]
            };
          } else if (!discoverResult.matches) {
            let { notFoundMatches, error, route } = handleNavigational404(
              location.pathname
            );
            return {
              matches: notFoundMatches,
              pendingActionResult: [
                route.id,
                {
                  type: "error",
                  error
                }
              ]
            };
          } else {
            matches = discoverResult.matches;
          }
        }
        let result;
        let actionMatch = getTargetMatch(matches, location);
        if (!actionMatch.route.action && !actionMatch.route.lazy) {
          result = {
            type: "error",
            error: getInternalRouterError(405, {
              method: request.method,
              pathname: location.pathname,
              routeId: actionMatch.route.id
            })
          };
        } else {
          let results = await callDataStrategy(
            "action",
            state,
            request,
            [actionMatch],
            matches,
            null
          );
          result = results[actionMatch.route.id];
          if (request.signal.aborted) {
            return { shortCircuited: true };
          }
        }
        if (isRedirectResult(result)) {
          let replace2;
          if (opts && opts.replace != null) {
            replace2 = opts.replace;
          } else {
            let location2 = normalizeRedirectLocation(
              result.response.headers.get("Location"),
              new URL(request.url),
              basename
            );
            replace2 = location2 === state.location.pathname + state.location.search;
          }
          await startRedirectNavigation(request, result, true, {
            submission,
            replace: replace2
          });
          return { shortCircuited: true };
        }
        if (isErrorResult(result)) {
          let boundaryMatch = findNearestBoundary(matches, actionMatch.route.id);
          if ((opts && opts.replace) !== true) {
            pendingAction = "PUSH";
          }
          return {
            matches,
            pendingActionResult: [boundaryMatch.route.id, result]
          };
        }
        return {
          matches,
          pendingActionResult: [actionMatch.route.id, result]
        };
      }
      async function handleLoaders(request, location, matches, isFogOfWar, overrideNavigation, submission, fetcherSubmission, replace2, initialHydration, flushSync, pendingActionResult) {
        let loadingNavigation = overrideNavigation || getLoadingNavigation(location, submission);
        let activeSubmission = submission || fetcherSubmission || getSubmissionFromNavigation(loadingNavigation);
        let shouldUpdateNavigationState = !isUninterruptedRevalidation && !initialHydration;
        if (isFogOfWar) {
          if (shouldUpdateNavigationState) {
            let actionData = getUpdatedActionData(pendingActionResult);
            updateState(
              {
                navigation: loadingNavigation,
                ...actionData !== void 0 ? { actionData } : {}
              },
              {
                flushSync
              }
            );
          }
          let discoverResult = await discoverRoutes(
            matches,
            location.pathname,
            request.signal
          );
          if (discoverResult.type === "aborted") {
            return { shortCircuited: true };
          } else if (discoverResult.type === "error") {
            let boundaryId = findNearestBoundary(discoverResult.partialMatches).route.id;
            return {
              matches: discoverResult.partialMatches,
              loaderData: {},
              errors: {
                [boundaryId]: discoverResult.error
              }
            };
          } else if (!discoverResult.matches) {
            let { error, notFoundMatches, route } = handleNavigational404(
              location.pathname
            );
            return {
              matches: notFoundMatches,
              loaderData: {},
              errors: {
                [route.id]: error
              }
            };
          } else {
            matches = discoverResult.matches;
          }
        }
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let [matchesToLoad, revalidatingFetchers] = getMatchesToLoad(
          init.history,
          state,
          matches,
          activeSubmission,
          location,
          initialHydration === true,
          isRevalidationRequired,
          cancelledFetcherLoads,
          fetchersQueuedForDeletion,
          fetchLoadMatches,
          fetchRedirectIds,
          routesToUse,
          basename,
          pendingActionResult
        );
        pendingNavigationLoadId = ++incrementingLoadId;
        if (matchesToLoad.length === 0 && revalidatingFetchers.length === 0) {
          let updatedFetchers2 = markFetchRedirectsDone();
          completeNavigation(
            location,
            {
              matches,
              loaderData: {},
              // Commit pending error if we're short circuiting
              errors: pendingActionResult && isErrorResult(pendingActionResult[1]) ? { [pendingActionResult[0]]: pendingActionResult[1].error } : null,
              ...getActionDataForCommit(pendingActionResult),
              ...updatedFetchers2 ? { fetchers: new Map(state.fetchers) } : {}
            },
            { flushSync }
          );
          return { shortCircuited: true };
        }
        if (shouldUpdateNavigationState) {
          let updates = {};
          if (!isFogOfWar) {
            updates.navigation = loadingNavigation;
            let actionData = getUpdatedActionData(pendingActionResult);
            if (actionData !== void 0) {
              updates.actionData = actionData;
            }
          }
          if (revalidatingFetchers.length > 0) {
            updates.fetchers = getUpdatedRevalidatingFetchers(revalidatingFetchers);
          }
          updateState(updates, { flushSync });
        }
        revalidatingFetchers.forEach((rf) => {
          abortFetcher(rf.key);
          if (rf.controller) {
            fetchControllers.set(rf.key, rf.controller);
          }
        });
        let abortPendingFetchRevalidations = () => revalidatingFetchers.forEach((f) => abortFetcher(f.key));
        if (pendingNavigationController) {
          pendingNavigationController.signal.addEventListener(
            "abort",
            abortPendingFetchRevalidations
          );
        }
        let { loaderResults, fetcherResults } = await callLoadersAndMaybeResolveData(
          state,
          matches,
          matchesToLoad,
          revalidatingFetchers,
          request
        );
        if (request.signal.aborted) {
          return { shortCircuited: true };
        }
        if (pendingNavigationController) {
          pendingNavigationController.signal.removeEventListener(
            "abort",
            abortPendingFetchRevalidations
          );
        }
        revalidatingFetchers.forEach((rf) => fetchControllers.delete(rf.key));
        let redirect2 = findRedirect(loaderResults);
        if (redirect2) {
          await startRedirectNavigation(request, redirect2.result, true, {
            replace: replace2
          });
          return { shortCircuited: true };
        }
        redirect2 = findRedirect(fetcherResults);
        if (redirect2) {
          fetchRedirectIds.add(redirect2.key);
          await startRedirectNavigation(request, redirect2.result, true, {
            replace: replace2
          });
          return { shortCircuited: true };
        }
        let { loaderData, errors } = processLoaderData(
          state,
          matches,
          loaderResults,
          pendingActionResult,
          revalidatingFetchers,
          fetcherResults
        );
        if (initialHydration && state.errors) {
          errors = { ...state.errors, ...errors };
        }
        let updatedFetchers = markFetchRedirectsDone();
        let didAbortFetchLoads = abortStaleFetchLoads(pendingNavigationLoadId);
        let shouldUpdateFetchers = updatedFetchers || didAbortFetchLoads || revalidatingFetchers.length > 0;
        return {
          matches,
          loaderData,
          errors,
          ...shouldUpdateFetchers ? { fetchers: new Map(state.fetchers) } : {}
        };
      }
      function getUpdatedActionData(pendingActionResult) {
        if (pendingActionResult && !isErrorResult(pendingActionResult[1])) {
          return {
            [pendingActionResult[0]]: pendingActionResult[1].data
          };
        } else if (state.actionData) {
          if (Object.keys(state.actionData).length === 0) {
            return null;
          } else {
            return state.actionData;
          }
        }
      }
      function getUpdatedRevalidatingFetchers(revalidatingFetchers) {
        revalidatingFetchers.forEach((rf) => {
          let fetcher = state.fetchers.get(rf.key);
          let revalidatingFetcher = getLoadingFetcher(
            void 0,
            fetcher ? fetcher.data : void 0
          );
          state.fetchers.set(rf.key, revalidatingFetcher);
        });
        return new Map(state.fetchers);
      }
      async function fetch2(key, routeId, href2, opts) {
        abortFetcher(key);
        let flushSync = (opts && opts.flushSync) === true;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let normalizedPath = normalizeTo(
          state.location,
          state.matches,
          basename,
          href2,
          routeId,
          opts == null ? void 0 : opts.relative
        );
        let matches = matchRoutes(routesToUse, normalizedPath, basename);
        let fogOfWar = checkFogOfWar(matches, routesToUse, normalizedPath);
        if (fogOfWar.active && fogOfWar.matches) {
          matches = fogOfWar.matches;
        }
        if (!matches) {
          setFetcherError(
            key,
            routeId,
            getInternalRouterError(404, { pathname: normalizedPath }),
            { flushSync }
          );
          return;
        }
        let { path, submission, error } = normalizeNavigateOptions(
          true,
          normalizedPath,
          opts
        );
        if (error) {
          setFetcherError(key, routeId, error, { flushSync });
          return;
        }
        let match = getTargetMatch(matches, path);
        let preventScrollReset = (opts && opts.preventScrollReset) === true;
        if (submission && isMutationMethod(submission.formMethod)) {
          await handleFetcherAction(
            key,
            routeId,
            path,
            match,
            matches,
            fogOfWar.active,
            flushSync,
            preventScrollReset,
            submission
          );
          return;
        }
        fetchLoadMatches.set(key, { routeId, path });
        await handleFetcherLoader(
          key,
          routeId,
          path,
          match,
          matches,
          fogOfWar.active,
          flushSync,
          preventScrollReset,
          submission
        );
      }
      async function handleFetcherAction(key, routeId, path, match, requestMatches, isFogOfWar, flushSync, preventScrollReset, submission) {
        interruptActiveLoads();
        fetchLoadMatches.delete(key);
        function detectAndHandle405Error(m) {
          if (!m.route.action && !m.route.lazy) {
            let error = getInternalRouterError(405, {
              method: submission.formMethod,
              pathname: path,
              routeId
            });
            setFetcherError(key, routeId, error, { flushSync });
            return true;
          }
          return false;
        }
        if (!isFogOfWar && detectAndHandle405Error(match)) {
          return;
        }
        let existingFetcher = state.fetchers.get(key);
        updateFetcherState(key, getSubmittingFetcher(submission, existingFetcher), {
          flushSync
        });
        let abortController = new AbortController();
        let fetchRequest = createClientSideRequest(
          init.history,
          path,
          abortController.signal,
          submission
        );
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            requestMatches,
            path,
            fetchRequest.signal
          );
          if (discoverResult.type === "aborted") {
            return;
          } else if (discoverResult.type === "error") {
            setFetcherError(key, routeId, discoverResult.error, { flushSync });
            return;
          } else if (!discoverResult.matches) {
            setFetcherError(
              key,
              routeId,
              getInternalRouterError(404, { pathname: path }),
              { flushSync }
            );
            return;
          } else {
            requestMatches = discoverResult.matches;
            match = getTargetMatch(requestMatches, path);
            if (detectAndHandle405Error(match)) {
              return;
            }
          }
        }
        fetchControllers.set(key, abortController);
        let originatingLoadId = incrementingLoadId;
        let actionResults = await callDataStrategy(
          "action",
          state,
          fetchRequest,
          [match],
          requestMatches,
          key
        );
        let actionResult = actionResults[match.route.id];
        if (fetchRequest.signal.aborted) {
          if (fetchControllers.get(key) === abortController) {
            fetchControllers.delete(key);
          }
          return;
        }
        if (fetchersQueuedForDeletion.has(key)) {
          if (isRedirectResult(actionResult) || isErrorResult(actionResult)) {
            updateFetcherState(key, getDoneFetcher(void 0));
            return;
          }
        } else {
          if (isRedirectResult(actionResult)) {
            fetchControllers.delete(key);
            if (pendingNavigationLoadId > originatingLoadId) {
              updateFetcherState(key, getDoneFetcher(void 0));
              return;
            } else {
              fetchRedirectIds.add(key);
              updateFetcherState(key, getLoadingFetcher(submission));
              return startRedirectNavigation(fetchRequest, actionResult, false, {
                fetcherSubmission: submission,
                preventScrollReset
              });
            }
          }
          if (isErrorResult(actionResult)) {
            setFetcherError(key, routeId, actionResult.error);
            return;
          }
        }
        let nextLocation = state.navigation.location || state.location;
        let revalidationRequest = createClientSideRequest(
          init.history,
          nextLocation,
          abortController.signal
        );
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let matches = state.navigation.state !== "idle" ? matchRoutes(routesToUse, state.navigation.location, basename) : state.matches;
        invariant(matches, "Didn't find any matches after fetcher action");
        let loadId = ++incrementingLoadId;
        fetchReloadIds.set(key, loadId);
        let loadFetcher = getLoadingFetcher(submission, actionResult.data);
        state.fetchers.set(key, loadFetcher);
        let [matchesToLoad, revalidatingFetchers] = getMatchesToLoad(
          init.history,
          state,
          matches,
          submission,
          nextLocation,
          false,
          isRevalidationRequired,
          cancelledFetcherLoads,
          fetchersQueuedForDeletion,
          fetchLoadMatches,
          fetchRedirectIds,
          routesToUse,
          basename,
          [match.route.id, actionResult]
        );
        revalidatingFetchers.filter((rf) => rf.key !== key).forEach((rf) => {
          let staleKey = rf.key;
          let existingFetcher2 = state.fetchers.get(staleKey);
          let revalidatingFetcher = getLoadingFetcher(
            void 0,
            existingFetcher2 ? existingFetcher2.data : void 0
          );
          state.fetchers.set(staleKey, revalidatingFetcher);
          abortFetcher(staleKey);
          if (rf.controller) {
            fetchControllers.set(staleKey, rf.controller);
          }
        });
        updateState({ fetchers: new Map(state.fetchers) });
        let abortPendingFetchRevalidations = () => revalidatingFetchers.forEach((rf) => abortFetcher(rf.key));
        abortController.signal.addEventListener(
          "abort",
          abortPendingFetchRevalidations
        );
        let { loaderResults, fetcherResults } = await callLoadersAndMaybeResolveData(
          state,
          matches,
          matchesToLoad,
          revalidatingFetchers,
          revalidationRequest
        );
        if (abortController.signal.aborted) {
          return;
        }
        abortController.signal.removeEventListener(
          "abort",
          abortPendingFetchRevalidations
        );
        fetchReloadIds.delete(key);
        fetchControllers.delete(key);
        revalidatingFetchers.forEach((r) => fetchControllers.delete(r.key));
        let redirect2 = findRedirect(loaderResults);
        if (redirect2) {
          return startRedirectNavigation(
            revalidationRequest,
            redirect2.result,
            false,
            { preventScrollReset }
          );
        }
        redirect2 = findRedirect(fetcherResults);
        if (redirect2) {
          fetchRedirectIds.add(redirect2.key);
          return startRedirectNavigation(
            revalidationRequest,
            redirect2.result,
            false,
            { preventScrollReset }
          );
        }
        let { loaderData, errors } = processLoaderData(
          state,
          matches,
          loaderResults,
          void 0,
          revalidatingFetchers,
          fetcherResults
        );
        if (state.fetchers.has(key)) {
          let doneFetcher = getDoneFetcher(actionResult.data);
          state.fetchers.set(key, doneFetcher);
        }
        abortStaleFetchLoads(loadId);
        if (state.navigation.state === "loading" && loadId > pendingNavigationLoadId) {
          invariant(pendingAction, "Expected pending action");
          pendingNavigationController && pendingNavigationController.abort();
          completeNavigation(state.navigation.location, {
            matches,
            loaderData,
            errors,
            fetchers: new Map(state.fetchers)
          });
        } else {
          updateState({
            errors,
            loaderData: mergeLoaderData(
              state.loaderData,
              loaderData,
              matches,
              errors
            ),
            fetchers: new Map(state.fetchers)
          });
          isRevalidationRequired = false;
        }
      }
      async function handleFetcherLoader(key, routeId, path, match, matches, isFogOfWar, flushSync, preventScrollReset, submission) {
        let existingFetcher = state.fetchers.get(key);
        updateFetcherState(
          key,
          getLoadingFetcher(
            submission,
            existingFetcher ? existingFetcher.data : void 0
          ),
          { flushSync }
        );
        let abortController = new AbortController();
        let fetchRequest = createClientSideRequest(
          init.history,
          path,
          abortController.signal
        );
        if (isFogOfWar) {
          let discoverResult = await discoverRoutes(
            matches,
            path,
            fetchRequest.signal
          );
          if (discoverResult.type === "aborted") {
            return;
          } else if (discoverResult.type === "error") {
            setFetcherError(key, routeId, discoverResult.error, { flushSync });
            return;
          } else if (!discoverResult.matches) {
            setFetcherError(
              key,
              routeId,
              getInternalRouterError(404, { pathname: path }),
              { flushSync }
            );
            return;
          } else {
            matches = discoverResult.matches;
            match = getTargetMatch(matches, path);
          }
        }
        fetchControllers.set(key, abortController);
        let originatingLoadId = incrementingLoadId;
        let results = await callDataStrategy(
          "loader",
          state,
          fetchRequest,
          [match],
          matches,
          key
        );
        let result = results[match.route.id];
        if (fetchControllers.get(key) === abortController) {
          fetchControllers.delete(key);
        }
        if (fetchRequest.signal.aborted) {
          return;
        }
        if (fetchersQueuedForDeletion.has(key)) {
          updateFetcherState(key, getDoneFetcher(void 0));
          return;
        }
        if (isRedirectResult(result)) {
          if (pendingNavigationLoadId > originatingLoadId) {
            updateFetcherState(key, getDoneFetcher(void 0));
            return;
          } else {
            fetchRedirectIds.add(key);
            await startRedirectNavigation(fetchRequest, result, false, {
              preventScrollReset
            });
            return;
          }
        }
        if (isErrorResult(result)) {
          setFetcherError(key, routeId, result.error);
          return;
        }
        updateFetcherState(key, getDoneFetcher(result.data));
      }
      async function startRedirectNavigation(request, redirect2, isNavigation, {
        submission,
        fetcherSubmission,
        preventScrollReset,
        replace: replace2
      } = {}) {
        if (redirect2.response.headers.has("X-Remix-Revalidate")) {
          isRevalidationRequired = true;
        }
        let location = redirect2.response.headers.get("Location");
        invariant(location, "Expected a Location header on the redirect Response");
        location = normalizeRedirectLocation(
          location,
          new URL(request.url),
          basename
        );
        let redirectLocation = createLocation(state.location, location, {
          _isRedirect: true
        });
        if (isBrowser2) {
          let isDocumentReload = false;
          if (redirect2.response.headers.has("X-Remix-Reload-Document")) {
            isDocumentReload = true;
          } else if (ABSOLUTE_URL_REGEX.test(location)) {
            const url = init.history.createURL(location);
            isDocumentReload = // Hard reload if it's an absolute URL to a new origin
            url.origin !== routerWindow.location.origin || // Hard reload if it's an absolute URL that does not match our basename
            stripBasename(url.pathname, basename) == null;
          }
          if (isDocumentReload) {
            if (replace2) {
              routerWindow.location.replace(location);
            } else {
              routerWindow.location.assign(location);
            }
            return;
          }
        }
        pendingNavigationController = null;
        let redirectNavigationType = replace2 === true || redirect2.response.headers.has("X-Remix-Replace") ? "REPLACE" : "PUSH";
        let { formMethod, formAction, formEncType } = state.navigation;
        if (!submission && !fetcherSubmission && formMethod && formAction && formEncType) {
          submission = getSubmissionFromNavigation(state.navigation);
        }
        let activeSubmission = submission || fetcherSubmission;
        if (redirectPreserveMethodStatusCodes.has(redirect2.response.status) && activeSubmission && isMutationMethod(activeSubmission.formMethod)) {
          await startNavigation(redirectNavigationType, redirectLocation, {
            submission: {
              ...activeSubmission,
              formAction: location
            },
            // Preserve these flags across redirects
            preventScrollReset: preventScrollReset || pendingPreventScrollReset,
            enableViewTransition: isNavigation ? pendingViewTransitionEnabled : void 0
          });
        } else {
          let overrideNavigation = getLoadingNavigation(
            redirectLocation,
            submission
          );
          await startNavigation(redirectNavigationType, redirectLocation, {
            overrideNavigation,
            // Send fetcher submissions through for shouldRevalidate
            fetcherSubmission,
            // Preserve these flags across redirects
            preventScrollReset: preventScrollReset || pendingPreventScrollReset,
            enableViewTransition: isNavigation ? pendingViewTransitionEnabled : void 0
          });
        }
      }
      async function callDataStrategy(type, state2, request, matchesToLoad, matches, fetcherKey) {
        let results;
        let dataResults = {};
        try {
          results = await callDataStrategyImpl(
            dataStrategyImpl,
            type,
            state2,
            request,
            matchesToLoad,
            matches,
            fetcherKey,
            manifest,
            mapRouteProperties2
          );
        } catch (e) {
          matchesToLoad.forEach((m) => {
            dataResults[m.route.id] = {
              type: "error",
              error: e
            };
          });
          return dataResults;
        }
        for (let [routeId, result] of Object.entries(results)) {
          if (isRedirectDataStrategyResult(result)) {
            let response = result.result;
            dataResults[routeId] = {
              type: "redirect",
              response: normalizeRelativeRoutingRedirectResponse(
                response,
                request,
                routeId,
                matches,
                basename
              )
            };
          } else {
            dataResults[routeId] = await convertDataStrategyResultToDataResult(
              result
            );
          }
        }
        return dataResults;
      }
      async function callLoadersAndMaybeResolveData(state2, matches, matchesToLoad, fetchersToLoad, request) {
        let loaderResultsPromise = callDataStrategy(
          "loader",
          state2,
          request,
          matchesToLoad,
          matches,
          null
        );
        let fetcherResultsPromise = Promise.all(
          fetchersToLoad.map(async (f) => {
            if (f.matches && f.match && f.controller) {
              let results = await callDataStrategy(
                "loader",
                state2,
                createClientSideRequest(init.history, f.path, f.controller.signal),
                [f.match],
                f.matches,
                f.key
              );
              let result = results[f.match.route.id];
              return { [f.key]: result };
            } else {
              return Promise.resolve({
                [f.key]: {
                  type: "error",
                  error: getInternalRouterError(404, {
                    pathname: f.path
                  })
                }
              });
            }
          })
        );
        let loaderResults = await loaderResultsPromise;
        let fetcherResults = (await fetcherResultsPromise).reduce(
          (acc, r) => Object.assign(acc, r),
          {}
        );
        return {
          loaderResults,
          fetcherResults
        };
      }
      function interruptActiveLoads() {
        isRevalidationRequired = true;
        fetchLoadMatches.forEach((_, key) => {
          if (fetchControllers.has(key)) {
            cancelledFetcherLoads.add(key);
          }
          abortFetcher(key);
        });
      }
      function updateFetcherState(key, fetcher, opts = {}) {
        state.fetchers.set(key, fetcher);
        updateState(
          { fetchers: new Map(state.fetchers) },
          { flushSync: (opts && opts.flushSync) === true }
        );
      }
      function setFetcherError(key, routeId, error, opts = {}) {
        let boundaryMatch = findNearestBoundary(state.matches, routeId);
        deleteFetcher(key);
        updateState(
          {
            errors: {
              [boundaryMatch.route.id]: error
            },
            fetchers: new Map(state.fetchers)
          },
          { flushSync: (opts && opts.flushSync) === true }
        );
      }
      function getFetcher(key) {
        activeFetchers.set(key, (activeFetchers.get(key) || 0) + 1);
        if (fetchersQueuedForDeletion.has(key)) {
          fetchersQueuedForDeletion.delete(key);
        }
        return state.fetchers.get(key) || IDLE_FETCHER;
      }
      function deleteFetcher(key) {
        let fetcher = state.fetchers.get(key);
        if (fetchControllers.has(key) && !(fetcher && fetcher.state === "loading" && fetchReloadIds.has(key))) {
          abortFetcher(key);
        }
        fetchLoadMatches.delete(key);
        fetchReloadIds.delete(key);
        fetchRedirectIds.delete(key);
        fetchersQueuedForDeletion.delete(key);
        cancelledFetcherLoads.delete(key);
        state.fetchers.delete(key);
      }
      function queueFetcherForDeletion(key) {
        let count = (activeFetchers.get(key) || 0) - 1;
        if (count <= 0) {
          activeFetchers.delete(key);
          fetchersQueuedForDeletion.add(key);
        } else {
          activeFetchers.set(key, count);
        }
        updateState({ fetchers: new Map(state.fetchers) });
      }
      function abortFetcher(key) {
        let controller = fetchControllers.get(key);
        if (controller) {
          controller.abort();
          fetchControllers.delete(key);
        }
      }
      function markFetchersDone(keys) {
        for (let key of keys) {
          let fetcher = getFetcher(key);
          let doneFetcher = getDoneFetcher(fetcher.data);
          state.fetchers.set(key, doneFetcher);
        }
      }
      function markFetchRedirectsDone() {
        let doneKeys = [];
        let updatedFetchers = false;
        for (let key of fetchRedirectIds) {
          let fetcher = state.fetchers.get(key);
          invariant(fetcher, `Expected fetcher: ${key}`);
          if (fetcher.state === "loading") {
            fetchRedirectIds.delete(key);
            doneKeys.push(key);
            updatedFetchers = true;
          }
        }
        markFetchersDone(doneKeys);
        return updatedFetchers;
      }
      function abortStaleFetchLoads(landedId) {
        let yeetedKeys = [];
        for (let [key, id] of fetchReloadIds) {
          if (id < landedId) {
            let fetcher = state.fetchers.get(key);
            invariant(fetcher, `Expected fetcher: ${key}`);
            if (fetcher.state === "loading") {
              abortFetcher(key);
              fetchReloadIds.delete(key);
              yeetedKeys.push(key);
            }
          }
        }
        markFetchersDone(yeetedKeys);
        return yeetedKeys.length > 0;
      }
      function getBlocker(key, fn) {
        let blocker = state.blockers.get(key) || IDLE_BLOCKER;
        if (blockerFunctions.get(key) !== fn) {
          blockerFunctions.set(key, fn);
        }
        return blocker;
      }
      function deleteBlocker(key) {
        state.blockers.delete(key);
        blockerFunctions.delete(key);
      }
      function updateBlocker(key, newBlocker) {
        let blocker = state.blockers.get(key) || IDLE_BLOCKER;
        invariant(
          blocker.state === "unblocked" && newBlocker.state === "blocked" || blocker.state === "blocked" && newBlocker.state === "blocked" || blocker.state === "blocked" && newBlocker.state === "proceeding" || blocker.state === "blocked" && newBlocker.state === "unblocked" || blocker.state === "proceeding" && newBlocker.state === "unblocked",
          `Invalid blocker state transition: ${blocker.state} -> ${newBlocker.state}`
        );
        let blockers = new Map(state.blockers);
        blockers.set(key, newBlocker);
        updateState({ blockers });
      }
      function shouldBlockNavigation({
        currentLocation,
        nextLocation,
        historyAction
      }) {
        if (blockerFunctions.size === 0) {
          return;
        }
        if (blockerFunctions.size > 1) {
          warning(false, "A router only supports one blocker at a time");
        }
        let entries = Array.from(blockerFunctions.entries());
        let [blockerKey, blockerFunction] = entries[entries.length - 1];
        let blocker = state.blockers.get(blockerKey);
        if (blocker && blocker.state === "proceeding") {
          return;
        }
        if (blockerFunction({ currentLocation, nextLocation, historyAction })) {
          return blockerKey;
        }
      }
      function handleNavigational404(pathname) {
        let error = getInternalRouterError(404, { pathname });
        let routesToUse = inFlightDataRoutes || dataRoutes;
        let { matches, route } = getShortCircuitMatches(routesToUse);
        return { notFoundMatches: matches, route, error };
      }
      function enableScrollRestoration(positions, getPosition, getKey) {
        savedScrollPositions2 = positions;
        getScrollPosition = getPosition;
        getScrollRestorationKey2 = getKey || null;
        if (!initialScrollRestored && state.navigation === IDLE_NAVIGATION) {
          initialScrollRestored = true;
          let y = getSavedScrollPosition(state.location, state.matches);
          if (y != null) {
            updateState({ restoreScrollPosition: y });
          }
        }
        return () => {
          savedScrollPositions2 = null;
          getScrollPosition = null;
          getScrollRestorationKey2 = null;
        };
      }
      function getScrollKey(location, matches) {
        if (getScrollRestorationKey2) {
          let key = getScrollRestorationKey2(
            location,
            matches.map((m) => convertRouteMatchToUiMatch(m, state.loaderData))
          );
          return key || location.key;
        }
        return location.key;
      }
      function saveScrollPosition(location, matches) {
        if (savedScrollPositions2 && getScrollPosition) {
          let key = getScrollKey(location, matches);
          savedScrollPositions2[key] = getScrollPosition();
        }
      }
      function getSavedScrollPosition(location, matches) {
        if (savedScrollPositions2) {
          let key = getScrollKey(location, matches);
          let y = savedScrollPositions2[key];
          if (typeof y === "number") {
            return y;
          }
        }
        return null;
      }
      function checkFogOfWar(matches, routesToUse, pathname) {
        if (patchRoutesOnNavigationImpl) {
          if (!matches) {
            let fogMatches = matchRoutesImpl(
              routesToUse,
              pathname,
              basename,
              true
            );
            return { active: true, matches: fogMatches || [] };
          } else {
            if (Object.keys(matches[0].params).length > 0) {
              let partialMatches = matchRoutesImpl(
                routesToUse,
                pathname,
                basename,
                true
              );
              return { active: true, matches: partialMatches };
            }
          }
        }
        return { active: false, matches: null };
      }
      async function discoverRoutes(matches, pathname, signal) {
        if (!patchRoutesOnNavigationImpl) {
          return { type: "success", matches };
        }
        let partialMatches = matches;
        while (true) {
          let isNonHMR = inFlightDataRoutes == null;
          let routesToUse = inFlightDataRoutes || dataRoutes;
          let localManifest = manifest;
          try {
            await patchRoutesOnNavigationImpl({
              signal,
              path: pathname,
              matches: partialMatches,
              patch: (routeId, children) => {
                if (signal.aborted) return;
                patchRoutesImpl(
                  routeId,
                  children,
                  routesToUse,
                  localManifest,
                  mapRouteProperties2
                );
              }
            });
          } catch (e) {
            return { type: "error", error: e, partialMatches };
          } finally {
            if (isNonHMR && !signal.aborted) {
              dataRoutes = [...dataRoutes];
            }
          }
          if (signal.aborted) {
            return { type: "aborted" };
          }
          let newMatches = matchRoutes(routesToUse, pathname, basename);
          if (newMatches) {
            return { type: "success", matches: newMatches };
          }
          let newPartialMatches = matchRoutesImpl(
            routesToUse,
            pathname,
            basename,
            true
          );
          if (!newPartialMatches || partialMatches.length === newPartialMatches.length && partialMatches.every(
            (m, i) => m.route.id === newPartialMatches[i].route.id
          )) {
            return { type: "success", matches: null };
          }
          partialMatches = newPartialMatches;
        }
      }
      function _internalSetRoutes(newRoutes) {
        manifest = {};
        inFlightDataRoutes = convertRoutesToDataRoutes(
          newRoutes,
          mapRouteProperties2,
          void 0,
          manifest
        );
      }
      function patchRoutes(routeId, children) {
        let isNonHMR = inFlightDataRoutes == null;
        let routesToUse = inFlightDataRoutes || dataRoutes;
        patchRoutesImpl(
          routeId,
          children,
          routesToUse,
          manifest,
          mapRouteProperties2
        );
        if (isNonHMR) {
          dataRoutes = [...dataRoutes];
          updateState({});
        }
      }
      router = {
        get basename() {
          return basename;
        },
        get future() {
          return future;
        },
        get state() {
          return state;
        },
        get routes() {
          return dataRoutes;
        },
        get window() {
          return routerWindow;
        },
        initialize,
        subscribe,
        enableScrollRestoration,
        navigate,
        fetch: fetch2,
        revalidate,
        // Passthrough to history-aware createHref used by useHref so we get proper
        // hash-aware URLs in DOM paths
        createHref: (to) => init.history.createHref(to),
        encodeLocation: (to) => init.history.encodeLocation(to),
        getFetcher,
        deleteFetcher: queueFetcherForDeletion,
        dispose,
        getBlocker,
        deleteBlocker,
        patchRoutes,
        _internalFetchControllers: fetchControllers,
        // TODO: Remove setRoutes, it's temporary to avoid dealing with
        // updating the tree while validating the update algorithm.
        _internalSetRoutes
      };
      return router;
    }
    function createStaticHandler(routes, opts) {
      invariant(
        routes.length > 0,
        "You must provide a non-empty routes array to createStaticHandler"
      );
      let manifest = {};
      let basename = (opts ? opts.basename : null) || "/";
      let mapRouteProperties2 = (opts == null ? void 0 : opts.mapRouteProperties) || defaultMapRouteProperties;
      let dataRoutes = convertRoutesToDataRoutes(
        routes,
        mapRouteProperties2,
        void 0,
        manifest
      );
      async function query(request, {
        requestContext,
        skipLoaderErrorBubbling,
        dataStrategy
      } = {}) {
        let url = new URL(request.url);
        let method = request.method;
        let location = createLocation("", createPath(url), null, "default");
        let matches = matchRoutes(dataRoutes, location, basename);
        if (!isValidMethod(method) && method !== "HEAD") {
          let error = getInternalRouterError(405, { method });
          let { matches: methodNotAllowedMatches, route } = getShortCircuitMatches(dataRoutes);
          return {
            basename,
            location,
            matches: methodNotAllowedMatches,
            loaderData: {},
            actionData: null,
            errors: {
              [route.id]: error
            },
            statusCode: error.status,
            loaderHeaders: {},
            actionHeaders: {}
          };
        } else if (!matches) {
          let error = getInternalRouterError(404, { pathname: location.pathname });
          let { matches: notFoundMatches, route } = getShortCircuitMatches(dataRoutes);
          return {
            basename,
            location,
            matches: notFoundMatches,
            loaderData: {},
            actionData: null,
            errors: {
              [route.id]: error
            },
            statusCode: error.status,
            loaderHeaders: {},
            actionHeaders: {}
          };
        }
        let result = await queryImpl(
          request,
          location,
          matches,
          requestContext,
          dataStrategy || null,
          skipLoaderErrorBubbling === true,
          null
        );
        if (isResponse(result)) {
          return result;
        }
        return { location, basename, ...result };
      }
      async function queryRoute(request, {
        routeId,
        requestContext,
        dataStrategy
      } = {}) {
        let url = new URL(request.url);
        let method = request.method;
        let location = createLocation("", createPath(url), null, "default");
        let matches = matchRoutes(dataRoutes, location, basename);
        if (!isValidMethod(method) && method !== "HEAD" && method !== "OPTIONS") {
          throw getInternalRouterError(405, { method });
        } else if (!matches) {
          throw getInternalRouterError(404, { pathname: location.pathname });
        }
        let match = routeId ? matches.find((m) => m.route.id === routeId) : getTargetMatch(matches, location);
        if (routeId && !match) {
          throw getInternalRouterError(403, {
            pathname: location.pathname,
            routeId
          });
        } else if (!match) {
          throw getInternalRouterError(404, { pathname: location.pathname });
        }
        let result = await queryImpl(
          request,
          location,
          matches,
          requestContext,
          dataStrategy || null,
          false,
          match
        );
        if (isResponse(result)) {
          return result;
        }
        let error = result.errors ? Object.values(result.errors)[0] : void 0;
        if (error !== void 0) {
          throw error;
        }
        if (result.actionData) {
          return Object.values(result.actionData)[0];
        }
        if (result.loaderData) {
          return Object.values(result.loaderData)[0];
        }
        return void 0;
      }
      async function queryImpl(request, location, matches, requestContext, dataStrategy, skipLoaderErrorBubbling, routeMatch) {
        invariant(
          request.signal,
          "query()/queryRoute() requests must contain an AbortController signal"
        );
        try {
          if (isMutationMethod(request.method)) {
            let result2 = await submit(
              request,
              matches,
              routeMatch || getTargetMatch(matches, location),
              requestContext,
              dataStrategy,
              skipLoaderErrorBubbling,
              routeMatch != null
            );
            return result2;
          }
          let result = await loadRouteData(
            request,
            matches,
            requestContext,
            dataStrategy,
            skipLoaderErrorBubbling,
            routeMatch
          );
          return isResponse(result) ? result : {
            ...result,
            actionData: null,
            actionHeaders: {}
          };
        } catch (e) {
          if (isDataStrategyResult(e) && isResponse(e.result)) {
            if (e.type === "error") {
              throw e.result;
            }
            return e.result;
          }
          if (isRedirectResponse(e)) {
            return e;
          }
          throw e;
        }
      }
      async function submit(request, matches, actionMatch, requestContext, dataStrategy, skipLoaderErrorBubbling, isRouteRequest) {
        let result;
        if (!actionMatch.route.action && !actionMatch.route.lazy) {
          let error = getInternalRouterError(405, {
            method: request.method,
            pathname: new URL(request.url).pathname,
            routeId: actionMatch.route.id
          });
          if (isRouteRequest) {
            throw error;
          }
          result = {
            type: "error",
            error
          };
        } else {
          let results = await callDataStrategy(
            "action",
            request,
            [actionMatch],
            matches,
            isRouteRequest,
            requestContext,
            dataStrategy
          );
          result = results[actionMatch.route.id];
          if (request.signal.aborted) {
            throwStaticHandlerAbortedError(request, isRouteRequest);
          }
        }
        if (isRedirectResult(result)) {
          throw new Response(null, {
            status: result.response.status,
            headers: {
              Location: result.response.headers.get("Location")
            }
          });
        }
        if (isRouteRequest) {
          if (isErrorResult(result)) {
            throw result.error;
          }
          return {
            matches: [actionMatch],
            loaderData: {},
            actionData: { [actionMatch.route.id]: result.data },
            errors: null,
            // Note: statusCode + headers are unused here since queryRoute will
            // return the raw Response or value
            statusCode: 200,
            loaderHeaders: {},
            actionHeaders: {}
          };
        }
        let loaderRequest = new Request(request.url, {
          headers: request.headers,
          redirect: request.redirect,
          signal: request.signal
        });
        if (isErrorResult(result)) {
          let boundaryMatch = skipLoaderErrorBubbling ? actionMatch : findNearestBoundary(matches, actionMatch.route.id);
          let context2 = await loadRouteData(
            loaderRequest,
            matches,
            requestContext,
            dataStrategy,
            skipLoaderErrorBubbling,
            null,
            [boundaryMatch.route.id, result]
          );
          return {
            ...context2,
            statusCode: isRouteErrorResponse(result.error) ? result.error.status : result.statusCode != null ? result.statusCode : 500,
            actionData: null,
            actionHeaders: {
              ...result.headers ? { [actionMatch.route.id]: result.headers } : {}
            }
          };
        }
        let context = await loadRouteData(
          loaderRequest,
          matches,
          requestContext,
          dataStrategy,
          skipLoaderErrorBubbling,
          null
        );
        return {
          ...context,
          actionData: {
            [actionMatch.route.id]: result.data
          },
          // action status codes take precedence over loader status codes
          ...result.statusCode ? { statusCode: result.statusCode } : {},
          actionHeaders: result.headers ? { [actionMatch.route.id]: result.headers } : {}
        };
      }
      async function loadRouteData(request, matches, requestContext, dataStrategy, skipLoaderErrorBubbling, routeMatch, pendingActionResult) {
        let isRouteRequest = routeMatch != null;
        if (isRouteRequest && !(routeMatch == null ? void 0 : routeMatch.route.loader) && !(routeMatch == null ? void 0 : routeMatch.route.lazy)) {
          throw getInternalRouterError(400, {
            method: request.method,
            pathname: new URL(request.url).pathname,
            routeId: routeMatch == null ? void 0 : routeMatch.route.id
          });
        }
        let requestMatches = routeMatch ? [routeMatch] : pendingActionResult && isErrorResult(pendingActionResult[1]) ? getLoaderMatchesUntilBoundary(matches, pendingActionResult[0]) : matches;
        let matchesToLoad = requestMatches.filter(
          (m) => m.route.loader || m.route.lazy
        );
        if (matchesToLoad.length === 0) {
          return {
            matches,
            // Add a null for all matched routes for proper revalidation on the client
            loaderData: matches.reduce(
              (acc, m) => Object.assign(acc, { [m.route.id]: null }),
              {}
            ),
            errors: pendingActionResult && isErrorResult(pendingActionResult[1]) ? {
              [pendingActionResult[0]]: pendingActionResult[1].error
            } : null,
            statusCode: 200,
            loaderHeaders: {}
          };
        }
        let results = await callDataStrategy(
          "loader",
          request,
          matchesToLoad,
          matches,
          isRouteRequest,
          requestContext,
          dataStrategy
        );
        if (request.signal.aborted) {
          throwStaticHandlerAbortedError(request, isRouteRequest);
        }
        let context = processRouteLoaderData(
          matches,
          results,
          pendingActionResult,
          true,
          skipLoaderErrorBubbling
        );
        let executedLoaders = new Set(
          matchesToLoad.map((match) => match.route.id)
        );
        matches.forEach((match) => {
          if (!executedLoaders.has(match.route.id)) {
            context.loaderData[match.route.id] = null;
          }
        });
        return {
          ...context,
          matches
        };
      }
      async function callDataStrategy(type, request, matchesToLoad, matches, isRouteRequest, requestContext, dataStrategy) {
        let results = await callDataStrategyImpl(
          dataStrategy || defaultDataStrategy,
          type,
          null,
          request,
          matchesToLoad,
          matches,
          null,
          manifest,
          mapRouteProperties2,
          requestContext
        );
        let dataResults = {};
        await Promise.all(
          matches.map(async (match) => {
            if (!(match.route.id in results)) {
              return;
            }
            let result = results[match.route.id];
            if (isRedirectDataStrategyResult(result)) {
              let response = result.result;
              throw normalizeRelativeRoutingRedirectResponse(
                response,
                request,
                match.route.id,
                matches,
                basename
              );
            }
            if (isResponse(result.result) && isRouteRequest) {
              throw result;
            }
            dataResults[match.route.id] = await convertDataStrategyResultToDataResult(result);
          })
        );
        return dataResults;
      }
      return {
        dataRoutes,
        query,
        queryRoute
      };
    }
    function getStaticContextFromError(routes, context, error) {
      let newContext = {
        ...context,
        statusCode: isRouteErrorResponse(error) ? error.status : 500,
        errors: {
          [context._deepestRenderedBoundaryId || routes[0].id]: error
        }
      };
      return newContext;
    }
    function throwStaticHandlerAbortedError(request, isRouteRequest) {
      if (request.signal.reason !== void 0) {
        throw request.signal.reason;
      }
      let method = isRouteRequest ? "queryRoute" : "query";
      throw new Error(
        `${method}() call aborted without an \`AbortSignal.reason\`: ${request.method} ${request.url}`
      );
    }
    function isSubmissionNavigation(opts) {
      return opts != null && ("formData" in opts && opts.formData != null || "body" in opts && opts.body !== void 0);
    }
    function normalizeTo(location, matches, basename, to, fromRouteId, relative) {
      let contextualMatches;
      let activeRouteMatch;
      if (fromRouteId) {
        contextualMatches = [];
        for (let match of matches) {
          contextualMatches.push(match);
          if (match.route.id === fromRouteId) {
            activeRouteMatch = match;
            break;
          }
        }
      } else {
        contextualMatches = matches;
        activeRouteMatch = matches[matches.length - 1];
      }
      let path = resolveTo(
        to ? to : ".",
        getResolveToMatches(contextualMatches),
        stripBasename(location.pathname, basename) || location.pathname,
        relative === "path"
      );
      if (to == null) {
        path.search = location.search;
        path.hash = location.hash;
      }
      if ((to == null || to === "" || to === ".") && activeRouteMatch) {
        let nakedIndex = hasNakedIndexQuery(path.search);
        if (activeRouteMatch.route.index && !nakedIndex) {
          path.search = path.search ? path.search.replace(/^\?/, "?index&") : "?index";
        } else if (!activeRouteMatch.route.index && nakedIndex) {
          let params = new URLSearchParams(path.search);
          let indexValues = params.getAll("index");
          params.delete("index");
          indexValues.filter((v) => v).forEach((v) => params.append("index", v));
          let qs = params.toString();
          path.search = qs ? `?${qs}` : "";
        }
      }
      if (basename !== "/") {
        path.pathname = path.pathname === "/" ? basename : joinPaths([basename, path.pathname]);
      }
      return createPath(path);
    }
    function normalizeNavigateOptions(isFetcher, path, opts) {
      if (!opts || !isSubmissionNavigation(opts)) {
        return { path };
      }
      if (opts.formMethod && !isValidMethod(opts.formMethod)) {
        return {
          path,
          error: getInternalRouterError(405, { method: opts.formMethod })
        };
      }
      let getInvalidBodyError = () => ({
        path,
        error: getInternalRouterError(400, { type: "invalid-body" })
      });
      let rawFormMethod = opts.formMethod || "get";
      let formMethod = rawFormMethod.toUpperCase();
      let formAction = stripHashFromPath(path);
      if (opts.body !== void 0) {
        if (opts.formEncType === "text/plain") {
          if (!isMutationMethod(formMethod)) {
            return getInvalidBodyError();
          }
          let text = typeof opts.body === "string" ? opts.body : opts.body instanceof FormData || opts.body instanceof URLSearchParams ? (
            // https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#plain-text-form-data
            Array.from(opts.body.entries()).reduce(
              (acc, [name, value]) => `${acc}${name}=${value}
`,
              ""
            )
          ) : String(opts.body);
          return {
            path,
            submission: {
              formMethod,
              formAction,
              formEncType: opts.formEncType,
              formData: void 0,
              json: void 0,
              text
            }
          };
        } else if (opts.formEncType === "application/json") {
          if (!isMutationMethod(formMethod)) {
            return getInvalidBodyError();
          }
          try {
            let json = typeof opts.body === "string" ? JSON.parse(opts.body) : opts.body;
            return {
              path,
              submission: {
                formMethod,
                formAction,
                formEncType: opts.formEncType,
                formData: void 0,
                json,
                text: void 0
              }
            };
          } catch (e) {
            return getInvalidBodyError();
          }
        }
      }
      invariant(
        typeof FormData === "function",
        "FormData is not available in this environment"
      );
      let searchParams;
      let formData;
      if (opts.formData) {
        searchParams = convertFormDataToSearchParams(opts.formData);
        formData = opts.formData;
      } else if (opts.body instanceof FormData) {
        searchParams = convertFormDataToSearchParams(opts.body);
        formData = opts.body;
      } else if (opts.body instanceof URLSearchParams) {
        searchParams = opts.body;
        formData = convertSearchParamsToFormData(searchParams);
      } else if (opts.body == null) {
        searchParams = new URLSearchParams();
        formData = new FormData();
      } else {
        try {
          searchParams = new URLSearchParams(opts.body);
          formData = convertSearchParamsToFormData(searchParams);
        } catch (e) {
          return getInvalidBodyError();
        }
      }
      let submission = {
        formMethod,
        formAction,
        formEncType: opts && opts.formEncType || "application/x-www-form-urlencoded",
        formData,
        json: void 0,
        text: void 0
      };
      if (isMutationMethod(submission.formMethod)) {
        return { path, submission };
      }
      let parsedPath = parsePath(path);
      if (isFetcher && parsedPath.search && hasNakedIndexQuery(parsedPath.search)) {
        searchParams.append("index", "");
      }
      parsedPath.search = `?${searchParams}`;
      return { path: createPath(parsedPath), submission };
    }
    function getLoaderMatchesUntilBoundary(matches, boundaryId, includeBoundary = false) {
      let index = matches.findIndex((m) => m.route.id === boundaryId);
      if (index >= 0) {
        return matches.slice(0, includeBoundary ? index + 1 : index);
      }
      return matches;
    }
    function getMatchesToLoad(history, state, matches, submission, location, initialHydration, isRevalidationRequired, cancelledFetcherLoads, fetchersQueuedForDeletion, fetchLoadMatches, fetchRedirectIds, routesToUse, basename, pendingActionResult) {
      let actionResult = pendingActionResult ? isErrorResult(pendingActionResult[1]) ? pendingActionResult[1].error : pendingActionResult[1].data : void 0;
      let currentUrl = history.createURL(state.location);
      let nextUrl = history.createURL(location);
      let boundaryMatches = matches;
      if (initialHydration && state.errors) {
        boundaryMatches = getLoaderMatchesUntilBoundary(
          matches,
          Object.keys(state.errors)[0],
          true
        );
      } else if (pendingActionResult && isErrorResult(pendingActionResult[1])) {
        boundaryMatches = getLoaderMatchesUntilBoundary(
          matches,
          pendingActionResult[0]
        );
      }
      let actionStatus = pendingActionResult ? pendingActionResult[1].statusCode : void 0;
      let shouldSkipRevalidation = actionStatus && actionStatus >= 400;
      let navigationMatches = boundaryMatches.filter((match, index) => {
        let { route } = match;
        if (route.lazy) {
          return true;
        }
        if (route.loader == null) {
          return false;
        }
        if (initialHydration) {
          return shouldLoadRouteOnHydration(route, state.loaderData, state.errors);
        }
        if (isNewLoader(state.loaderData, state.matches[index], match)) {
          return true;
        }
        let currentRouteMatch = state.matches[index];
        let nextRouteMatch = match;
        return shouldRevalidateLoader(match, {
          currentUrl,
          currentParams: currentRouteMatch.params,
          nextUrl,
          nextParams: nextRouteMatch.params,
          ...submission,
          actionResult,
          actionStatus,
          defaultShouldRevalidate: shouldSkipRevalidation ? false : (
            // Forced revalidation due to submission, useRevalidator, or X-Remix-Revalidate
            isRevalidationRequired || currentUrl.pathname + currentUrl.search === nextUrl.pathname + nextUrl.search || // Search params affect all loaders
            currentUrl.search !== nextUrl.search || isNewRouteInstance(currentRouteMatch, nextRouteMatch)
          )
        });
      });
      let revalidatingFetchers = [];
      fetchLoadMatches.forEach((f, key) => {
        if (initialHydration || !matches.some((m) => m.route.id === f.routeId) || fetchersQueuedForDeletion.has(key)) {
          return;
        }
        let fetcherMatches = matchRoutes(routesToUse, f.path, basename);
        if (!fetcherMatches) {
          revalidatingFetchers.push({
            key,
            routeId: f.routeId,
            path: f.path,
            matches: null,
            match: null,
            controller: null
          });
          return;
        }
        let fetcher = state.fetchers.get(key);
        let fetcherMatch = getTargetMatch(fetcherMatches, f.path);
        let shouldRevalidate = false;
        if (fetchRedirectIds.has(key)) {
          shouldRevalidate = false;
        } else if (cancelledFetcherLoads.has(key)) {
          cancelledFetcherLoads.delete(key);
          shouldRevalidate = true;
        } else if (fetcher && fetcher.state !== "idle" && fetcher.data === void 0) {
          shouldRevalidate = isRevalidationRequired;
        } else {
          shouldRevalidate = shouldRevalidateLoader(fetcherMatch, {
            currentUrl,
            currentParams: state.matches[state.matches.length - 1].params,
            nextUrl,
            nextParams: matches[matches.length - 1].params,
            ...submission,
            actionResult,
            actionStatus,
            defaultShouldRevalidate: shouldSkipRevalidation ? false : isRevalidationRequired
          });
        }
        if (shouldRevalidate) {
          revalidatingFetchers.push({
            key,
            routeId: f.routeId,
            path: f.path,
            matches: fetcherMatches,
            match: fetcherMatch,
            controller: new AbortController()
          });
        }
      });
      return [navigationMatches, revalidatingFetchers];
    }
    function shouldLoadRouteOnHydration(route, loaderData, errors) {
      if (route.lazy) {
        return true;
      }
      if (!route.loader) {
        return false;
      }
      let hasData = loaderData != null && loaderData[route.id] !== void 0;
      let hasError = errors != null && errors[route.id] !== void 0;
      if (!hasData && hasError) {
        return false;
      }
      if (typeof route.loader === "function" && route.loader.hydrate === true) {
        return true;
      }
      return !hasData && !hasError;
    }
    function isNewLoader(currentLoaderData, currentMatch, match) {
      let isNew = (
        // [a] -> [a, b]
        !currentMatch || // [a, b] -> [a, c]
        match.route.id !== currentMatch.route.id
      );
      let isMissingData = !currentLoaderData.hasOwnProperty(match.route.id);
      return isNew || isMissingData;
    }
    function isNewRouteInstance(currentMatch, match) {
      let currentPath = currentMatch.route.path;
      return (
        // param change for this match, /users/123 -> /users/456
        currentMatch.pathname !== match.pathname || // splat param changed, which is not present in match.path
        // e.g. /files/images/avatar.jpg -> files/finances.xls
        currentPath != null && currentPath.endsWith("*") && currentMatch.params["*"] !== match.params["*"]
      );
    }
    function shouldRevalidateLoader(loaderMatch, arg) {
      if (loaderMatch.route.shouldRevalidate) {
        let routeChoice = loaderMatch.route.shouldRevalidate(arg);
        if (typeof routeChoice === "boolean") {
          return routeChoice;
        }
      }
      return arg.defaultShouldRevalidate;
    }
    function patchRoutesImpl(routeId, children, routesToUse, manifest, mapRouteProperties2) {
      let childrenToPatch;
      if (routeId) {
        let route = manifest[routeId];
        invariant(
          route,
          `No route found to patch children into: routeId = ${routeId}`
        );
        if (!route.children) {
          route.children = [];
        }
        childrenToPatch = route.children;
      } else {
        childrenToPatch = routesToUse;
      }
      let uniqueChildren = children.filter(
        (newRoute) => !childrenToPatch.some(
          (existingRoute) => isSameRoute(newRoute, existingRoute)
        )
      );
      let newRoutes = convertRoutesToDataRoutes(
        uniqueChildren,
        mapRouteProperties2,
        [routeId || "_", "patch", String((childrenToPatch == null ? void 0 : childrenToPatch.length) || "0")],
        manifest
      );
      childrenToPatch.push(...newRoutes);
    }
    function isSameRoute(newRoute, existingRoute) {
      if ("id" in newRoute && "id" in existingRoute && newRoute.id === existingRoute.id) {
        return true;
      }
      if (!(newRoute.index === existingRoute.index && newRoute.path === existingRoute.path && newRoute.caseSensitive === existingRoute.caseSensitive)) {
        return false;
      }
      if ((!newRoute.children || newRoute.children.length === 0) && (!existingRoute.children || existingRoute.children.length === 0)) {
        return true;
      }
      return newRoute.children.every(
        (aChild, i) => {
          var _a;
          return (_a = existingRoute.children) == null ? void 0 : _a.some((bChild) => isSameRoute(aChild, bChild));
        }
      );
    }
    async function loadLazyRouteModule(route, mapRouteProperties2, manifest) {
      if (!route.lazy) {
        return;
      }
      let lazyRoute = await route.lazy();
      if (!route.lazy) {
        return;
      }
      let routeToUpdate = manifest[route.id];
      invariant(routeToUpdate, "No route found in manifest");
      let routeUpdates = {};
      for (let lazyRouteProperty in lazyRoute) {
        let staticRouteValue = routeToUpdate[lazyRouteProperty];
        let isPropertyStaticallyDefined = staticRouteValue !== void 0 && // This property isn't static since it should always be updated based
        // on the route updates
        lazyRouteProperty !== "hasErrorBoundary";
        warning(
          !isPropertyStaticallyDefined,
          `Route "${routeToUpdate.id}" has a static property "${lazyRouteProperty}" defined but its lazy function is also returning a value for this property. The lazy route property "${lazyRouteProperty}" will be ignored.`
        );
        if (!isPropertyStaticallyDefined && !immutableRouteKeys.has(lazyRouteProperty)) {
          routeUpdates[lazyRouteProperty] = lazyRoute[lazyRouteProperty];
        }
      }
      Object.assign(routeToUpdate, routeUpdates);
      Object.assign(routeToUpdate, {
        // To keep things framework agnostic, we use the provided `mapRouteProperties`
        // function to set the framework-aware properties (`element`/`hasErrorBoundary`)
        // since the logic will differ between frameworks.
        ...mapRouteProperties2(routeToUpdate),
        lazy: void 0
      });
    }
    async function defaultDataStrategy({
      matches
    }) {
      let matchesToLoad = matches.filter((m) => m.shouldLoad);
      let results = await Promise.all(matchesToLoad.map((m) => m.resolve()));
      return results.reduce(
        (acc, result, i) => Object.assign(acc, { [matchesToLoad[i].route.id]: result }),
        {}
      );
    }
    async function callDataStrategyImpl(dataStrategyImpl, type, state, request, matchesToLoad, matches, fetcherKey, manifest, mapRouteProperties2, requestContext) {
      let loadRouteDefinitionsPromises = matches.map(
        (m) => m.route.lazy ? loadLazyRouteModule(m.route, mapRouteProperties2, manifest) : void 0
      );
      let dsMatches = matches.map((match, i) => {
        let loadRoutePromise = loadRouteDefinitionsPromises[i];
        let shouldLoad = matchesToLoad.some((m) => m.route.id === match.route.id);
        let resolve = async (handlerOverride) => {
          if (handlerOverride && request.method === "GET" && (match.route.lazy || match.route.loader)) {
            shouldLoad = true;
          }
          return shouldLoad ? callLoaderOrAction(
            type,
            request,
            match,
            loadRoutePromise,
            handlerOverride,
            requestContext
          ) : Promise.resolve({ type: "data", result: void 0 });
        };
        return {
          ...match,
          shouldLoad,
          resolve
        };
      });
      let results = await dataStrategyImpl({
        matches: dsMatches,
        request,
        params: matches[0].params,
        fetcherKey,
        context: requestContext
      });
      try {
        await Promise.all(loadRouteDefinitionsPromises);
      } catch (e) {
      }
      return results;
    }
    async function callLoaderOrAction(type, request, match, loadRoutePromise, handlerOverride, staticContext) {
      let result;
      let onReject;
      let runHandler = (handler) => {
        let reject;
        let abortPromise = new Promise((_, r) => reject = r);
        onReject = () => reject();
        request.signal.addEventListener("abort", onReject);
        let actualHandler = (ctx) => {
          if (typeof handler !== "function") {
            return Promise.reject(
              new Error(
                `You cannot call the handler for a route which defines a boolean "${type}" [routeId: ${match.route.id}]`
              )
            );
          }
          return handler(
            {
              request,
              params: match.params,
              context: staticContext
            },
            ...ctx !== void 0 ? [ctx] : []
          );
        };
        let handlerPromise = (async () => {
          try {
            let val = await (handlerOverride ? handlerOverride((ctx) => actualHandler(ctx)) : actualHandler());
            return { type: "data", result: val };
          } catch (e) {
            return { type: "error", result: e };
          }
        })();
        return Promise.race([handlerPromise, abortPromise]);
      };
      try {
        let handler = match.route[type];
        if (loadRoutePromise) {
          if (handler) {
            let handlerError;
            let [value] = await Promise.all([
              // If the handler throws, don't let it immediately bubble out,
              // since we need to let the lazy() execution finish so we know if this
              // route has a boundary that can handle the error
              runHandler(handler).catch((e) => {
                handlerError = e;
              }),
              loadRoutePromise
            ]);
            if (handlerError !== void 0) {
              throw handlerError;
            }
            result = value;
          } else {
            await loadRoutePromise;
            handler = match.route[type];
            if (handler) {
              result = await runHandler(handler);
            } else if (type === "action") {
              let url = new URL(request.url);
              let pathname = url.pathname + url.search;
              throw getInternalRouterError(405, {
                method: request.method,
                pathname,
                routeId: match.route.id
              });
            } else {
              return { type: "data", result: void 0 };
            }
          }
        } else if (!handler) {
          let url = new URL(request.url);
          let pathname = url.pathname + url.search;
          throw getInternalRouterError(404, {
            pathname
          });
        } else {
          result = await runHandler(handler);
        }
      } catch (e) {
        return { type: "error", result: e };
      } finally {
        if (onReject) {
          request.signal.removeEventListener("abort", onReject);
        }
      }
      return result;
    }
    async function convertDataStrategyResultToDataResult(dataStrategyResult) {
      var _a, _b, _c, _d, _e, _f;
      let { result, type } = dataStrategyResult;
      if (isResponse(result)) {
        let data2;
        try {
          let contentType = result.headers.get("Content-Type");
          if (contentType && /\bapplication\/json\b/.test(contentType)) {
            if (result.body == null) {
              data2 = null;
            } else {
              data2 = await result.json();
            }
          } else {
            data2 = await result.text();
          }
        } catch (e) {
          return { type: "error", error: e };
        }
        if (type === "error") {
          return {
            type: "error",
            error: new ErrorResponseImpl(result.status, result.statusText, data2),
            statusCode: result.status,
            headers: result.headers
          };
        }
        return {
          type: "data",
          data: data2,
          statusCode: result.status,
          headers: result.headers
        };
      }
      if (type === "error") {
        if (isDataWithResponseInit(result)) {
          if (result.data instanceof Error) {
            return {
              type: "error",
              error: result.data,
              statusCode: (_a = result.init) == null ? void 0 : _a.status,
              headers: ((_b = result.init) == null ? void 0 : _b.headers) ? new Headers(result.init.headers) : void 0
            };
          }
          return {
            type: "error",
            error: new ErrorResponseImpl(
              ((_c = result.init) == null ? void 0 : _c.status) || 500,
              void 0,
              result.data
            ),
            statusCode: isRouteErrorResponse(result) ? result.status : void 0,
            headers: ((_d = result.init) == null ? void 0 : _d.headers) ? new Headers(result.init.headers) : void 0
          };
        }
        return {
          type: "error",
          error: result,
          statusCode: isRouteErrorResponse(result) ? result.status : void 0
        };
      }
      if (isDataWithResponseInit(result)) {
        return {
          type: "data",
          data: result.data,
          statusCode: (_e = result.init) == null ? void 0 : _e.status,
          headers: ((_f = result.init) == null ? void 0 : _f.headers) ? new Headers(result.init.headers) : void 0
        };
      }
      return { type: "data", data: result };
    }
    function normalizeRelativeRoutingRedirectResponse(response, request, routeId, matches, basename) {
      let location = response.headers.get("Location");
      invariant(
        location,
        "Redirects returned/thrown from loaders/actions must have a Location header"
      );
      if (!ABSOLUTE_URL_REGEX.test(location)) {
        let trimmedMatches = matches.slice(
          0,
          matches.findIndex((m) => m.route.id === routeId) + 1
        );
        location = normalizeTo(
          new URL(request.url),
          trimmedMatches,
          basename,
          location
        );
        response.headers.set("Location", location);
      }
      return response;
    }
    function normalizeRedirectLocation(location, currentUrl, basename) {
      if (ABSOLUTE_URL_REGEX.test(location)) {
        let normalizedLocation = location;
        let url = normalizedLocation.startsWith("//") ? new URL(currentUrl.protocol + normalizedLocation) : new URL(normalizedLocation);
        let isSameBasename = stripBasename(url.pathname, basename) != null;
        if (url.origin === currentUrl.origin && isSameBasename) {
          return url.pathname + url.search + url.hash;
        }
      }
      return location;
    }
    function createClientSideRequest(history, location, signal, submission) {
      let url = history.createURL(stripHashFromPath(location)).toString();
      let init = { signal };
      if (submission && isMutationMethod(submission.formMethod)) {
        let { formMethod, formEncType } = submission;
        init.method = formMethod.toUpperCase();
        if (formEncType === "application/json") {
          init.headers = new Headers({ "Content-Type": formEncType });
          init.body = JSON.stringify(submission.json);
        } else if (formEncType === "text/plain") {
          init.body = submission.text;
        } else if (formEncType === "application/x-www-form-urlencoded" && submission.formData) {
          init.body = convertFormDataToSearchParams(submission.formData);
        } else {
          init.body = submission.formData;
        }
      }
      return new Request(url, init);
    }
    function convertFormDataToSearchParams(formData) {
      let searchParams = new URLSearchParams();
      for (let [key, value] of formData.entries()) {
        searchParams.append(key, typeof value === "string" ? value : value.name);
      }
      return searchParams;
    }
    function convertSearchParamsToFormData(searchParams) {
      let formData = new FormData();
      for (let [key, value] of searchParams.entries()) {
        formData.append(key, value);
      }
      return formData;
    }
    function processRouteLoaderData(matches, results, pendingActionResult, isStaticHandler = false, skipLoaderErrorBubbling = false) {
      let loaderData = {};
      let errors = null;
      let statusCode;
      let foundError = false;
      let loaderHeaders = {};
      let pendingError = pendingActionResult && isErrorResult(pendingActionResult[1]) ? pendingActionResult[1].error : void 0;
      matches.forEach((match) => {
        if (!(match.route.id in results)) {
          return;
        }
        let id = match.route.id;
        let result = results[id];
        invariant(
          !isRedirectResult(result),
          "Cannot handle redirect results in processLoaderData"
        );
        if (isErrorResult(result)) {
          let error = result.error;
          if (pendingError !== void 0) {
            error = pendingError;
            pendingError = void 0;
          }
          errors = errors || {};
          if (skipLoaderErrorBubbling) {
            errors[id] = error;
          } else {
            let boundaryMatch = findNearestBoundary(matches, id);
            if (errors[boundaryMatch.route.id] == null) {
              errors[boundaryMatch.route.id] = error;
            }
          }
          if (!isStaticHandler) {
            loaderData[id] = ResetLoaderDataSymbol;
          }
          if (!foundError) {
            foundError = true;
            statusCode = isRouteErrorResponse(result.error) ? result.error.status : 500;
          }
          if (result.headers) {
            loaderHeaders[id] = result.headers;
          }
        } else {
          loaderData[id] = result.data;
          if (result.statusCode && result.statusCode !== 200 && !foundError) {
            statusCode = result.statusCode;
          }
          if (result.headers) {
            loaderHeaders[id] = result.headers;
          }
        }
      });
      if (pendingError !== void 0 && pendingActionResult) {
        errors = { [pendingActionResult[0]]: pendingError };
        loaderData[pendingActionResult[0]] = void 0;
      }
      return {
        loaderData,
        errors,
        statusCode: statusCode || 200,
        loaderHeaders
      };
    }
    function processLoaderData(state, matches, results, pendingActionResult, revalidatingFetchers, fetcherResults) {
      let { loaderData, errors } = processRouteLoaderData(
        matches,
        results,
        pendingActionResult
      );
      revalidatingFetchers.forEach((rf) => {
        let { key, match, controller } = rf;
        let result = fetcherResults[key];
        invariant(result, "Did not find corresponding fetcher result");
        if (controller && controller.signal.aborted) {
          return;
        } else if (isErrorResult(result)) {
          let boundaryMatch = findNearestBoundary(state.matches, match == null ? void 0 : match.route.id);
          if (!(errors && errors[boundaryMatch.route.id])) {
            errors = {
              ...errors,
              [boundaryMatch.route.id]: result.error
            };
          }
          state.fetchers.delete(key);
        } else if (isRedirectResult(result)) {
          invariant(false, "Unhandled fetcher revalidation redirect");
        } else {
          let doneFetcher = getDoneFetcher(result.data);
          state.fetchers.set(key, doneFetcher);
        }
      });
      return { loaderData, errors };
    }
    function mergeLoaderData(loaderData, newLoaderData, matches, errors) {
      let mergedLoaderData = Object.entries(newLoaderData).filter(([, v]) => v !== ResetLoaderDataSymbol).reduce((merged, [k, v]) => {
        merged[k] = v;
        return merged;
      }, {});
      for (let match of matches) {
        let id = match.route.id;
        if (!newLoaderData.hasOwnProperty(id) && loaderData.hasOwnProperty(id) && match.route.loader) {
          mergedLoaderData[id] = loaderData[id];
        }
        if (errors && errors.hasOwnProperty(id)) {
          break;
        }
      }
      return mergedLoaderData;
    }
    function getActionDataForCommit(pendingActionResult) {
      if (!pendingActionResult) {
        return {};
      }
      return isErrorResult(pendingActionResult[1]) ? {
        // Clear out prior actionData on errors
        actionData: {}
      } : {
        actionData: {
          [pendingActionResult[0]]: pendingActionResult[1].data
        }
      };
    }
    function findNearestBoundary(matches, routeId) {
      let eligibleMatches = routeId ? matches.slice(0, matches.findIndex((m) => m.route.id === routeId) + 1) : [...matches];
      return eligibleMatches.reverse().find((m) => m.route.hasErrorBoundary === true) || matches[0];
    }
    function getShortCircuitMatches(routes) {
      let route = routes.length === 1 ? routes[0] : routes.find((r) => r.index || !r.path || r.path === "/") || {
        id: `__shim-error-route__`
      };
      return {
        matches: [
          {
            params: {},
            pathname: "",
            pathnameBase: "",
            route
          }
        ],
        route
      };
    }
    function getInternalRouterError(status, {
      pathname,
      routeId,
      method,
      type,
      message
    } = {}) {
      let statusText = "Unknown Server Error";
      let errorMessage = "Unknown @remix-run/router error";
      if (status === 400) {
        statusText = "Bad Request";
        if (method && pathname && routeId) {
          errorMessage = `You made a ${method} request to "${pathname}" but did not provide a \`loader\` for route "${routeId}", so there is no way to handle the request.`;
        } else if (type === "invalid-body") {
          errorMessage = "Unable to encode submission body";
        }
      } else if (status === 403) {
        statusText = "Forbidden";
        errorMessage = `Route "${routeId}" does not match URL "${pathname}"`;
      } else if (status === 404) {
        statusText = "Not Found";
        errorMessage = `No route matches URL "${pathname}"`;
      } else if (status === 405) {
        statusText = "Method Not Allowed";
        if (method && pathname && routeId) {
          errorMessage = `You made a ${method.toUpperCase()} request to "${pathname}" but did not provide an \`action\` for route "${routeId}", so there is no way to handle the request.`;
        } else if (method) {
          errorMessage = `Invalid request method "${method.toUpperCase()}"`;
        }
      }
      return new ErrorResponseImpl(
        status || 500,
        statusText,
        new Error(errorMessage),
        true
      );
    }
    function findRedirect(results) {
      let entries = Object.entries(results);
      for (let i = entries.length - 1; i >= 0; i--) {
        let [key, result] = entries[i];
        if (isRedirectResult(result)) {
          return { key, result };
        }
      }
    }
    function stripHashFromPath(path) {
      let parsedPath = typeof path === "string" ? parsePath(path) : path;
      return createPath({ ...parsedPath, hash: "" });
    }
    function isHashChangeOnly(a, b) {
      if (a.pathname !== b.pathname || a.search !== b.search) {
        return false;
      }
      if (a.hash === "") {
        return b.hash !== "";
      } else if (a.hash === b.hash) {
        return true;
      } else if (b.hash !== "") {
        return true;
      }
      return false;
    }
    function isDataStrategyResult(result) {
      return result != null && typeof result === "object" && "type" in result && "result" in result && (result.type === "data" || result.type === "error");
    }
    function isRedirectDataStrategyResult(result) {
      return isResponse(result.result) && redirectStatusCodes.has(result.result.status);
    }
    function isErrorResult(result) {
      return result.type === "error";
    }
    function isRedirectResult(result) {
      return (result && result.type) === "redirect";
    }
    function isDataWithResponseInit(value) {
      return typeof value === "object" && value != null && "type" in value && "data" in value && "init" in value && value.type === "DataWithResponseInit";
    }
    function isResponse(value) {
      return value != null && typeof value.status === "number" && typeof value.statusText === "string" && typeof value.headers === "object" && typeof value.body !== "undefined";
    }
    function isRedirectStatusCode(statusCode) {
      return redirectStatusCodes.has(statusCode);
    }
    function isRedirectResponse(result) {
      return isResponse(result) && isRedirectStatusCode(result.status) && result.headers.has("Location");
    }
    function isValidMethod(method) {
      return validRequestMethods.has(method.toUpperCase());
    }
    function isMutationMethod(method) {
      return validMutationMethods.has(method.toUpperCase());
    }
    function hasNakedIndexQuery(search) {
      return new URLSearchParams(search).getAll("index").some((v) => v === "");
    }
    function getTargetMatch(matches, location) {
      let search = typeof location === "string" ? parsePath(location).search : location.search;
      if (matches[matches.length - 1].route.index && hasNakedIndexQuery(search || "")) {
        return matches[matches.length - 1];
      }
      let pathMatches = getPathContributingMatches(matches);
      return pathMatches[pathMatches.length - 1];
    }
    function getSubmissionFromNavigation(navigation) {
      let { formMethod, formAction, formEncType, text, formData, json } = navigation;
      if (!formMethod || !formAction || !formEncType) {
        return;
      }
      if (text != null) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData: void 0,
          json: void 0,
          text
        };
      } else if (formData != null) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData,
          json: void 0,
          text: void 0
        };
      } else if (json !== void 0) {
        return {
          formMethod,
          formAction,
          formEncType,
          formData: void 0,
          json,
          text: void 0
        };
      }
    }
    function getLoadingNavigation(location, submission) {
      if (submission) {
        let navigation = {
          state: "loading",
          location,
          formMethod: submission.formMethod,
          formAction: submission.formAction,
          formEncType: submission.formEncType,
          formData: submission.formData,
          json: submission.json,
          text: submission.text
        };
        return navigation;
      } else {
        let navigation = {
          state: "loading",
          location,
          formMethod: void 0,
          formAction: void 0,
          formEncType: void 0,
          formData: void 0,
          json: void 0,
          text: void 0
        };
        return navigation;
      }
    }
    function getSubmittingNavigation(location, submission) {
      let navigation = {
        state: "submitting",
        location,
        formMethod: submission.formMethod,
        formAction: submission.formAction,
        formEncType: submission.formEncType,
        formData: submission.formData,
        json: submission.json,
        text: submission.text
      };
      return navigation;
    }
    function getLoadingFetcher(submission, data2) {
      if (submission) {
        let fetcher = {
          state: "loading",
          formMethod: submission.formMethod,
          formAction: submission.formAction,
          formEncType: submission.formEncType,
          formData: submission.formData,
          json: submission.json,
          text: submission.text,
          data: data2
        };
        return fetcher;
      } else {
        let fetcher = {
          state: "loading",
          formMethod: void 0,
          formAction: void 0,
          formEncType: void 0,
          formData: void 0,
          json: void 0,
          text: void 0,
          data: data2
        };
        return fetcher;
      }
    }
    function getSubmittingFetcher(submission, existingFetcher) {
      let fetcher = {
        state: "submitting",
        formMethod: submission.formMethod,
        formAction: submission.formAction,
        formEncType: submission.formEncType,
        formData: submission.formData,
        json: submission.json,
        text: submission.text,
        data: existingFetcher ? existingFetcher.data : void 0
      };
      return fetcher;
    }
    function getDoneFetcher(data2) {
      let fetcher = {
        state: "idle",
        formMethod: void 0,
        formAction: void 0,
        formEncType: void 0,
        formData: void 0,
        json: void 0,
        text: void 0,
        data: data2
      };
      return fetcher;
    }
    function restoreAppliedTransitions(_window, transitions) {
      try {
        let sessionPositions = _window.sessionStorage.getItem(
          TRANSITIONS_STORAGE_KEY
        );
        if (sessionPositions) {
          let json = JSON.parse(sessionPositions);
          for (let [k, v] of Object.entries(json || {})) {
            if (v && Array.isArray(v)) {
              transitions.set(k, new Set(v || []));
            }
          }
        }
      } catch (e) {
      }
    }
    function persistAppliedTransitions(_window, transitions) {
      if (transitions.size > 0) {
        let json = {};
        for (let [k, v] of transitions) {
          json[k] = [...v];
        }
        try {
          _window.sessionStorage.setItem(
            TRANSITIONS_STORAGE_KEY,
            JSON.stringify(json)
          );
        } catch (error) {
          warning(
            false,
            `Failed to save applied view transitions in sessionStorage (${error}).`
          );
        }
      }
    }
    function createDeferred() {
      let resolve;
      let reject;
      let promise = new Promise((res, rej) => {
        resolve = async (val) => {
          res(val);
          try {
            await promise;
          } catch (e) {
          }
        };
        reject = async (error) => {
          rej(error);
          try {
            await promise;
          } catch (e) {
          }
        };
      });
      return {
        promise,
        //@ts-ignore
        resolve,
        //@ts-ignore
        reject
      };
    }
    var React3 = __toESM2(require_react());
    var React2 = __toESM2(require_react());
    var DataRouterContext = React2.createContext(null);
    DataRouterContext.displayName = "DataRouter";
    var DataRouterStateContext = React2.createContext(null);
    DataRouterStateContext.displayName = "DataRouterState";
    var ViewTransitionContext = React2.createContext({
      isTransitioning: false
    });
    ViewTransitionContext.displayName = "ViewTransition";
    var FetchersContext = React2.createContext(
      /* @__PURE__ */ new Map()
    );
    FetchersContext.displayName = "Fetchers";
    var AwaitContext = React2.createContext(null);
    AwaitContext.displayName = "Await";
    var NavigationContext = React2.createContext(
      null
    );
    NavigationContext.displayName = "Navigation";
    var LocationContext = React2.createContext(
      null
    );
    LocationContext.displayName = "Location";
    var RouteContext = React2.createContext({
      outlet: null,
      matches: [],
      isDataRoute: false
    });
    RouteContext.displayName = "Route";
    var RouteErrorContext = React2.createContext(null);
    RouteErrorContext.displayName = "RouteError";
    var React22 = __toESM2(require_react());
    var ENABLE_DEV_WARNINGS = true;
    function useHref(to, { relative } = {}) {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useHref() may be used only in the context of a <Router> component.`
      );
      let { basename, navigator: navigator2 } = React22.useContext(NavigationContext);
      let { hash, pathname, search } = useResolvedPath(to, { relative });
      let joinedPathname = pathname;
      if (basename !== "/") {
        joinedPathname = pathname === "/" ? basename : joinPaths([basename, pathname]);
      }
      return navigator2.createHref({ pathname: joinedPathname, search, hash });
    }
    function useInRouterContext() {
      return React22.useContext(LocationContext) != null;
    }
    function useLocation() {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useLocation() may be used only in the context of a <Router> component.`
      );
      return React22.useContext(LocationContext).location;
    }
    function useNavigationType() {
      return React22.useContext(LocationContext).navigationType;
    }
    function useMatch(pattern) {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useMatch() may be used only in the context of a <Router> component.`
      );
      let { pathname } = useLocation();
      return React22.useMemo(
        () => matchPath(pattern, decodePath(pathname)),
        [pathname, pattern]
      );
    }
    var navigateEffectWarning = `You should call navigate() in a React.useEffect(), not when your component is first rendered.`;
    function useIsomorphicLayoutEffect(cb) {
      let isStatic = React22.useContext(NavigationContext).static;
      if (!isStatic) {
        React22.useLayoutEffect(cb);
      }
    }
    function useNavigate() {
      let { isDataRoute } = React22.useContext(RouteContext);
      return isDataRoute ? useNavigateStable() : useNavigateUnstable();
    }
    function useNavigateUnstable() {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useNavigate() may be used only in the context of a <Router> component.`
      );
      let dataRouterContext = React22.useContext(DataRouterContext);
      let { basename, navigator: navigator2 } = React22.useContext(NavigationContext);
      let { matches } = React22.useContext(RouteContext);
      let { pathname: locationPathname } = useLocation();
      let routePathnamesJson = JSON.stringify(getResolveToMatches(matches));
      let activeRef = React22.useRef(false);
      useIsomorphicLayoutEffect(() => {
        activeRef.current = true;
      });
      let navigate = React22.useCallback(
        (to, options = {}) => {
          warning(activeRef.current, navigateEffectWarning);
          if (!activeRef.current) return;
          if (typeof to === "number") {
            navigator2.go(to);
            return;
          }
          let path = resolveTo(
            to,
            JSON.parse(routePathnamesJson),
            locationPathname,
            options.relative === "path"
          );
          if (dataRouterContext == null && basename !== "/") {
            path.pathname = path.pathname === "/" ? basename : joinPaths([basename, path.pathname]);
          }
          (!!options.replace ? navigator2.replace : navigator2.push)(
            path,
            options.state,
            options
          );
        },
        [
          basename,
          navigator2,
          routePathnamesJson,
          locationPathname,
          dataRouterContext
        ]
      );
      return navigate;
    }
    var OutletContext = React22.createContext(null);
    function useOutletContext() {
      return React22.useContext(OutletContext);
    }
    function useOutlet(context) {
      let outlet = React22.useContext(RouteContext).outlet;
      if (outlet) {
        return React22.createElement(OutletContext.Provider, { value: context }, outlet);
      }
      return outlet;
    }
    function useParams() {
      let { matches } = React22.useContext(RouteContext);
      let routeMatch = matches[matches.length - 1];
      return routeMatch ? routeMatch.params : {};
    }
    function useResolvedPath(to, { relative } = {}) {
      let { matches } = React22.useContext(RouteContext);
      let { pathname: locationPathname } = useLocation();
      let routePathnamesJson = JSON.stringify(getResolveToMatches(matches));
      return React22.useMemo(
        () => resolveTo(
          to,
          JSON.parse(routePathnamesJson),
          locationPathname,
          relative === "path"
        ),
        [to, routePathnamesJson, locationPathname, relative]
      );
    }
    function useRoutes(routes, locationArg) {
      return useRoutesImpl(routes, locationArg);
    }
    function useRoutesImpl(routes, locationArg, dataRouterState, future) {
      var _a;
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of the
        // router loaded. We can help them understand how to avoid that.
        `useRoutes() may be used only in the context of a <Router> component.`
      );
      let { navigator: navigator2, static: isStatic } = React22.useContext(NavigationContext);
      let { matches: parentMatches } = React22.useContext(RouteContext);
      let routeMatch = parentMatches[parentMatches.length - 1];
      let parentParams = routeMatch ? routeMatch.params : {};
      let parentPathname = routeMatch ? routeMatch.pathname : "/";
      let parentPathnameBase = routeMatch ? routeMatch.pathnameBase : "/";
      let parentRoute = routeMatch && routeMatch.route;
      if (ENABLE_DEV_WARNINGS) {
        let parentPath = parentRoute && parentRoute.path || "";
        warningOnce(
          parentPathname,
          !parentRoute || parentPath.endsWith("*") || parentPath.endsWith("*?"),
          `You rendered descendant <Routes> (or called \`useRoutes()\`) at "${parentPathname}" (under <Route path="${parentPath}">) but the parent route path has no trailing "*". This means if you navigate deeper, the parent won't match anymore and therefore the child routes will never render.

Please change the parent <Route path="${parentPath}"> to <Route path="${parentPath === "/" ? "*" : `${parentPath}/*`}">.`
        );
      }
      let locationFromContext = useLocation();
      let location;
      if (locationArg) {
        let parsedLocationArg = typeof locationArg === "string" ? parsePath(locationArg) : locationArg;
        invariant(
          parentPathnameBase === "/" || ((_a = parsedLocationArg.pathname) == null ? void 0 : _a.startsWith(parentPathnameBase)),
          `When overriding the location using \`<Routes location>\` or \`useRoutes(routes, location)\`, the location pathname must begin with the portion of the URL pathname that was matched by all parent routes. The current pathname base is "${parentPathnameBase}" but pathname "${parsedLocationArg.pathname}" was given in the \`location\` prop.`
        );
        location = parsedLocationArg;
      } else {
        location = locationFromContext;
      }
      let pathname = location.pathname || "/";
      let remainingPathname = pathname;
      if (parentPathnameBase !== "/") {
        let parentSegments = parentPathnameBase.replace(/^\//, "").split("/");
        let segments = pathname.replace(/^\//, "").split("/");
        remainingPathname = "/" + segments.slice(parentSegments.length).join("/");
      }
      let matches = !isStatic && dataRouterState && dataRouterState.matches && dataRouterState.matches.length > 0 ? dataRouterState.matches : matchRoutes(routes, { pathname: remainingPathname });
      if (ENABLE_DEV_WARNINGS) {
        warning(
          parentRoute || matches != null,
          `No routes matched location "${location.pathname}${location.search}${location.hash}" `
        );
        warning(
          matches == null || matches[matches.length - 1].route.element !== void 0 || matches[matches.length - 1].route.Component !== void 0 || matches[matches.length - 1].route.lazy !== void 0,
          `Matched leaf route at location "${location.pathname}${location.search}${location.hash}" does not have an element or Component. This means it will render an <Outlet /> with a null value by default resulting in an "empty" page.`
        );
      }
      let renderedMatches = _renderMatches(
        matches && matches.map(
          (match) => Object.assign({}, match, {
            params: Object.assign({}, parentParams, match.params),
            pathname: joinPaths([
              parentPathnameBase,
              // Re-encode pathnames that were decoded inside matchRoutes
              navigator2.encodeLocation ? navigator2.encodeLocation(match.pathname).pathname : match.pathname
            ]),
            pathnameBase: match.pathnameBase === "/" ? parentPathnameBase : joinPaths([
              parentPathnameBase,
              // Re-encode pathnames that were decoded inside matchRoutes
              navigator2.encodeLocation ? navigator2.encodeLocation(match.pathnameBase).pathname : match.pathnameBase
            ])
          })
        ),
        parentMatches,
        dataRouterState,
        future
      );
      if (locationArg && renderedMatches) {
        return React22.createElement(
          LocationContext.Provider,
          {
            value: {
              location: {
                pathname: "/",
                search: "",
                hash: "",
                state: null,
                key: "default",
                ...location
              },
              navigationType: "POP"
              /* Pop */
            }
          },
          renderedMatches
        );
      }
      return renderedMatches;
    }
    function DefaultErrorComponent() {
      let error = useRouteError();
      let message = isRouteErrorResponse(error) ? `${error.status} ${error.statusText}` : error instanceof Error ? error.message : JSON.stringify(error);
      let stack = error instanceof Error ? error.stack : null;
      let lightgrey = "rgba(200,200,200, 0.5)";
      let preStyles = { padding: "0.5rem", backgroundColor: lightgrey };
      let codeStyles = { padding: "2px 4px", backgroundColor: lightgrey };
      let devInfo = null;
      if (ENABLE_DEV_WARNINGS) {
        console.error(
          "Error handled by React Router default ErrorBoundary:",
          error
        );
        devInfo = React22.createElement(React22.Fragment, null, React22.createElement("p", null, "💿 Hey developer 👋"), React22.createElement("p", null, "You can provide a way better UX than this when your app throws errors by providing your own ", React22.createElement("code", { style: codeStyles }, "ErrorBoundary"), " or", " ", React22.createElement("code", { style: codeStyles }, "errorElement"), " prop on your route."));
      }
      return React22.createElement(React22.Fragment, null, React22.createElement("h2", null, "Unexpected Application Error!"), React22.createElement("h3", { style: { fontStyle: "italic" } }, message), stack ? React22.createElement("pre", { style: preStyles }, stack) : null, devInfo);
    }
    var defaultErrorElement = React22.createElement(DefaultErrorComponent, null);
    var RenderErrorBoundary = class extends React22.Component {
      constructor(props) {
        super(props);
        this.state = {
          location: props.location,
          revalidation: props.revalidation,
          error: props.error
        };
      }
      static getDerivedStateFromError(error) {
        return { error };
      }
      static getDerivedStateFromProps(props, state) {
        if (state.location !== props.location || state.revalidation !== "idle" && props.revalidation === "idle") {
          return {
            error: props.error,
            location: props.location,
            revalidation: props.revalidation
          };
        }
        return {
          error: props.error !== void 0 ? props.error : state.error,
          location: state.location,
          revalidation: props.revalidation || state.revalidation
        };
      }
      componentDidCatch(error, errorInfo) {
        console.error(
          "React Router caught the following error during render",
          error,
          errorInfo
        );
      }
      render() {
        return this.state.error !== void 0 ? React22.createElement(RouteContext.Provider, { value: this.props.routeContext }, React22.createElement(
          RouteErrorContext.Provider,
          {
            value: this.state.error,
            children: this.props.component
          }
        )) : this.props.children;
      }
    };
    function RenderedRoute({ routeContext, match, children }) {
      let dataRouterContext = React22.useContext(DataRouterContext);
      if (dataRouterContext && dataRouterContext.static && dataRouterContext.staticContext && (match.route.errorElement || match.route.ErrorBoundary)) {
        dataRouterContext.staticContext._deepestRenderedBoundaryId = match.route.id;
      }
      return React22.createElement(RouteContext.Provider, { value: routeContext }, children);
    }
    function _renderMatches(matches, parentMatches = [], dataRouterState = null, future = null) {
      if (matches == null) {
        if (!dataRouterState) {
          return null;
        }
        if (dataRouterState.errors) {
          matches = dataRouterState.matches;
        } else if (parentMatches.length === 0 && !dataRouterState.initialized && dataRouterState.matches.length > 0) {
          matches = dataRouterState.matches;
        } else {
          return null;
        }
      }
      let renderedMatches = matches;
      let errors = dataRouterState == null ? void 0 : dataRouterState.errors;
      if (errors != null) {
        let errorIndex = renderedMatches.findIndex(
          (m) => m.route.id && (errors == null ? void 0 : errors[m.route.id]) !== void 0
        );
        invariant(
          errorIndex >= 0,
          `Could not find a matching route for errors on route IDs: ${Object.keys(
            errors
          ).join(",")}`
        );
        renderedMatches = renderedMatches.slice(
          0,
          Math.min(renderedMatches.length, errorIndex + 1)
        );
      }
      let renderFallback = false;
      let fallbackIndex = -1;
      if (dataRouterState) {
        for (let i = 0; i < renderedMatches.length; i++) {
          let match = renderedMatches[i];
          if (match.route.HydrateFallback || match.route.hydrateFallbackElement) {
            fallbackIndex = i;
          }
          if (match.route.id) {
            let { loaderData, errors: errors2 } = dataRouterState;
            let needsToRunLoader = match.route.loader && !loaderData.hasOwnProperty(match.route.id) && (!errors2 || errors2[match.route.id] === void 0);
            if (match.route.lazy || needsToRunLoader) {
              renderFallback = true;
              if (fallbackIndex >= 0) {
                renderedMatches = renderedMatches.slice(0, fallbackIndex + 1);
              } else {
                renderedMatches = [renderedMatches[0]];
              }
              break;
            }
          }
        }
      }
      return renderedMatches.reduceRight((outlet, match, index) => {
        let error;
        let shouldRenderHydrateFallback = false;
        let errorElement = null;
        let hydrateFallbackElement = null;
        if (dataRouterState) {
          error = errors && match.route.id ? errors[match.route.id] : void 0;
          errorElement = match.route.errorElement || defaultErrorElement;
          if (renderFallback) {
            if (fallbackIndex < 0 && index === 0) {
              warningOnce(
                "route-fallback",
                false,
                "No `HydrateFallback` element provided to render during initial hydration"
              );
              shouldRenderHydrateFallback = true;
              hydrateFallbackElement = null;
            } else if (fallbackIndex === index) {
              shouldRenderHydrateFallback = true;
              hydrateFallbackElement = match.route.hydrateFallbackElement || null;
            }
          }
        }
        let matches2 = parentMatches.concat(renderedMatches.slice(0, index + 1));
        let getChildren = () => {
          let children;
          if (error) {
            children = errorElement;
          } else if (shouldRenderHydrateFallback) {
            children = hydrateFallbackElement;
          } else if (match.route.Component) {
            children = React22.createElement(match.route.Component, null);
          } else if (match.route.element) {
            children = match.route.element;
          } else {
            children = outlet;
          }
          return React22.createElement(
            RenderedRoute,
            {
              match,
              routeContext: {
                outlet,
                matches: matches2,
                isDataRoute: dataRouterState != null
              },
              children
            }
          );
        };
        return dataRouterState && (match.route.ErrorBoundary || match.route.errorElement || index === 0) ? React22.createElement(
          RenderErrorBoundary,
          {
            location: dataRouterState.location,
            revalidation: dataRouterState.revalidation,
            component: errorElement,
            error,
            children: getChildren(),
            routeContext: { outlet: null, matches: matches2, isDataRoute: true }
          }
        ) : getChildren();
      }, null);
    }
    function getDataRouterConsoleError(hookName) {
      return `${hookName} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`;
    }
    function useDataRouterContext(hookName) {
      let ctx = React22.useContext(DataRouterContext);
      invariant(ctx, getDataRouterConsoleError(hookName));
      return ctx;
    }
    function useDataRouterState(hookName) {
      let state = React22.useContext(DataRouterStateContext);
      invariant(state, getDataRouterConsoleError(hookName));
      return state;
    }
    function useRouteContext(hookName) {
      let route = React22.useContext(RouteContext);
      invariant(route, getDataRouterConsoleError(hookName));
      return route;
    }
    function useCurrentRouteId(hookName) {
      let route = useRouteContext(hookName);
      let thisRoute = route.matches[route.matches.length - 1];
      invariant(
        thisRoute.route.id,
        `${hookName} can only be used on routes that contain a unique "id"`
      );
      return thisRoute.route.id;
    }
    function useRouteId() {
      return useCurrentRouteId(
        "useRouteId"
        /* UseRouteId */
      );
    }
    function useNavigation() {
      let state = useDataRouterState(
        "useNavigation"
        /* UseNavigation */
      );
      return state.navigation;
    }
    function useRevalidator() {
      let dataRouterContext = useDataRouterContext(
        "useRevalidator"
        /* UseRevalidator */
      );
      let state = useDataRouterState(
        "useRevalidator"
        /* UseRevalidator */
      );
      return React22.useMemo(
        () => ({
          async revalidate() {
            await dataRouterContext.router.revalidate();
          },
          state: state.revalidation
        }),
        [dataRouterContext.router, state.revalidation]
      );
    }
    function useMatches() {
      let { matches, loaderData } = useDataRouterState(
        "useMatches"
        /* UseMatches */
      );
      return React22.useMemo(
        () => matches.map((m) => convertRouteMatchToUiMatch(m, loaderData)),
        [matches, loaderData]
      );
    }
    function useLoaderData() {
      let state = useDataRouterState(
        "useLoaderData"
        /* UseLoaderData */
      );
      let routeId = useCurrentRouteId(
        "useLoaderData"
        /* UseLoaderData */
      );
      return state.loaderData[routeId];
    }
    function useRouteLoaderData(routeId) {
      let state = useDataRouterState(
        "useRouteLoaderData"
        /* UseRouteLoaderData */
      );
      return state.loaderData[routeId];
    }
    function useActionData() {
      let state = useDataRouterState(
        "useActionData"
        /* UseActionData */
      );
      let routeId = useCurrentRouteId(
        "useLoaderData"
        /* UseLoaderData */
      );
      return state.actionData ? state.actionData[routeId] : void 0;
    }
    function useRouteError() {
      var _a;
      let error = React22.useContext(RouteErrorContext);
      let state = useDataRouterState(
        "useRouteError"
        /* UseRouteError */
      );
      let routeId = useCurrentRouteId(
        "useRouteError"
        /* UseRouteError */
      );
      if (error !== void 0) {
        return error;
      }
      return (_a = state.errors) == null ? void 0 : _a[routeId];
    }
    function useAsyncValue() {
      let value = React22.useContext(AwaitContext);
      return value == null ? void 0 : value._data;
    }
    function useAsyncError() {
      let value = React22.useContext(AwaitContext);
      return value == null ? void 0 : value._error;
    }
    var blockerId = 0;
    function useBlocker(shouldBlock) {
      let { router, basename } = useDataRouterContext(
        "useBlocker"
        /* UseBlocker */
      );
      let state = useDataRouterState(
        "useBlocker"
        /* UseBlocker */
      );
      let [blockerKey, setBlockerKey] = React22.useState("");
      let blockerFunction = React22.useCallback(
        (arg) => {
          if (typeof shouldBlock !== "function") {
            return !!shouldBlock;
          }
          if (basename === "/") {
            return shouldBlock(arg);
          }
          let { currentLocation, nextLocation, historyAction } = arg;
          return shouldBlock({
            currentLocation: {
              ...currentLocation,
              pathname: stripBasename(currentLocation.pathname, basename) || currentLocation.pathname
            },
            nextLocation: {
              ...nextLocation,
              pathname: stripBasename(nextLocation.pathname, basename) || nextLocation.pathname
            },
            historyAction
          });
        },
        [basename, shouldBlock]
      );
      React22.useEffect(() => {
        let key = String(++blockerId);
        setBlockerKey(key);
        return () => router.deleteBlocker(key);
      }, [router]);
      React22.useEffect(() => {
        if (blockerKey !== "") {
          router.getBlocker(blockerKey, blockerFunction);
        }
      }, [router, blockerKey, blockerFunction]);
      return blockerKey && state.blockers.has(blockerKey) ? state.blockers.get(blockerKey) : IDLE_BLOCKER;
    }
    function useNavigateStable() {
      let { router } = useDataRouterContext(
        "useNavigate"
        /* UseNavigateStable */
      );
      let id = useCurrentRouteId(
        "useNavigate"
        /* UseNavigateStable */
      );
      let activeRef = React22.useRef(false);
      useIsomorphicLayoutEffect(() => {
        activeRef.current = true;
      });
      let navigate = React22.useCallback(
        async (to, options = {}) => {
          warning(activeRef.current, navigateEffectWarning);
          if (!activeRef.current) return;
          if (typeof to === "number") {
            router.navigate(to);
          } else {
            await router.navigate(to, { fromRouteId: id, ...options });
          }
        },
        [router, id]
      );
      return navigate;
    }
    var alreadyWarned = {};
    function warningOnce(key, cond, message) {
      if (!cond && !alreadyWarned[key]) {
        alreadyWarned[key] = true;
        warning(false, message);
      }
    }
    var alreadyWarned2 = {};
    function warnOnce(condition, message) {
      if (!condition && !alreadyWarned2[message]) {
        alreadyWarned2[message] = true;
        console.warn(message);
      }
    }
    var ENABLE_DEV_WARNINGS2 = true;
    function mapRouteProperties(route) {
      let updates = {
        // Note: this check also occurs in createRoutesFromChildren so update
        // there if you change this -- please and thank you!
        hasErrorBoundary: route.hasErrorBoundary || route.ErrorBoundary != null || route.errorElement != null
      };
      if (route.Component) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.element) {
            warning(
              false,
              "You should not include both `Component` and `element` on your route - `Component` will be used."
            );
          }
        }
        Object.assign(updates, {
          element: React3.createElement(route.Component),
          Component: void 0
        });
      }
      if (route.HydrateFallback) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.hydrateFallbackElement) {
            warning(
              false,
              "You should not include both `HydrateFallback` and `hydrateFallbackElement` on your route - `HydrateFallback` will be used."
            );
          }
        }
        Object.assign(updates, {
          hydrateFallbackElement: React3.createElement(route.HydrateFallback),
          HydrateFallback: void 0
        });
      }
      if (route.ErrorBoundary) {
        if (ENABLE_DEV_WARNINGS2) {
          if (route.errorElement) {
            warning(
              false,
              "You should not include both `ErrorBoundary` and `errorElement` on your route - `ErrorBoundary` will be used."
            );
          }
        }
        Object.assign(updates, {
          errorElement: React3.createElement(route.ErrorBoundary),
          ErrorBoundary: void 0
        });
      }
      return updates;
    }
    function createMemoryRouter(routes, opts) {
      return createRouter({
        basename: opts == null ? void 0 : opts.basename,
        future: opts == null ? void 0 : opts.future,
        history: createMemoryHistory({
          initialEntries: opts == null ? void 0 : opts.initialEntries,
          initialIndex: opts == null ? void 0 : opts.initialIndex
        }),
        hydrationData: opts == null ? void 0 : opts.hydrationData,
        routes,
        mapRouteProperties,
        dataStrategy: opts == null ? void 0 : opts.dataStrategy,
        patchRoutesOnNavigation: opts == null ? void 0 : opts.patchRoutesOnNavigation
      }).initialize();
    }
    var Deferred = class {
      constructor() {
        this.status = "pending";
        this.promise = new Promise((resolve, reject) => {
          this.resolve = (value) => {
            if (this.status === "pending") {
              this.status = "resolved";
              resolve(value);
            }
          };
          this.reject = (reason) => {
            if (this.status === "pending") {
              this.status = "rejected";
              reject(reason);
            }
          };
        });
      }
    };
    function RouterProvider2({
      router,
      flushSync: reactDomFlushSyncImpl
    }) {
      let [state, setStateImpl] = React3.useState(router.state);
      let [pendingState, setPendingState] = React3.useState();
      let [vtContext, setVtContext] = React3.useState({
        isTransitioning: false
      });
      let [renderDfd, setRenderDfd] = React3.useState();
      let [transition, setTransition] = React3.useState();
      let [interruption, setInterruption] = React3.useState();
      let fetcherData = React3.useRef(/* @__PURE__ */ new Map());
      let setState = React3.useCallback(
        (newState, { deletedFetchers, flushSync, viewTransitionOpts }) => {
          newState.fetchers.forEach((fetcher, key) => {
            if (fetcher.data !== void 0) {
              fetcherData.current.set(key, fetcher.data);
            }
          });
          deletedFetchers.forEach((key) => fetcherData.current.delete(key));
          warnOnce(
            flushSync === false || reactDomFlushSyncImpl != null,
            'You provided the `flushSync` option to a router update, but you are not using the `<RouterProvider>` from `react-router/dom` so `ReactDOM.flushSync()` is unavailable.  Please update your app to `import { RouterProvider } from "react-router/dom"` and ensure you have `react-dom` installed as a dependency to use the `flushSync` option.'
          );
          let isViewTransitionAvailable = router.window != null && router.window.document != null && typeof router.window.document.startViewTransition === "function";
          warnOnce(
            viewTransitionOpts == null || isViewTransitionAvailable,
            "You provided the `viewTransition` option to a router update, but you do not appear to be running in a DOM environment as `window.startViewTransition` is not available."
          );
          if (!viewTransitionOpts || !isViewTransitionAvailable) {
            if (reactDomFlushSyncImpl && flushSync) {
              reactDomFlushSyncImpl(() => setStateImpl(newState));
            } else {
              React3.startTransition(() => setStateImpl(newState));
            }
            return;
          }
          if (reactDomFlushSyncImpl && flushSync) {
            reactDomFlushSyncImpl(() => {
              if (transition) {
                renderDfd && renderDfd.resolve();
                transition.skipTransition();
              }
              setVtContext({
                isTransitioning: true,
                flushSync: true,
                currentLocation: viewTransitionOpts.currentLocation,
                nextLocation: viewTransitionOpts.nextLocation
              });
            });
            let t = router.window.document.startViewTransition(() => {
              reactDomFlushSyncImpl(() => setStateImpl(newState));
            });
            t.finished.finally(() => {
              reactDomFlushSyncImpl(() => {
                setRenderDfd(void 0);
                setTransition(void 0);
                setPendingState(void 0);
                setVtContext({ isTransitioning: false });
              });
            });
            reactDomFlushSyncImpl(() => setTransition(t));
            return;
          }
          if (transition) {
            renderDfd && renderDfd.resolve();
            transition.skipTransition();
            setInterruption({
              state: newState,
              currentLocation: viewTransitionOpts.currentLocation,
              nextLocation: viewTransitionOpts.nextLocation
            });
          } else {
            setPendingState(newState);
            setVtContext({
              isTransitioning: true,
              flushSync: false,
              currentLocation: viewTransitionOpts.currentLocation,
              nextLocation: viewTransitionOpts.nextLocation
            });
          }
        },
        [router.window, reactDomFlushSyncImpl, transition, renderDfd]
      );
      React3.useLayoutEffect(() => router.subscribe(setState), [router, setState]);
      React3.useEffect(() => {
        if (vtContext.isTransitioning && !vtContext.flushSync) {
          setRenderDfd(new Deferred());
        }
      }, [vtContext]);
      React3.useEffect(() => {
        if (renderDfd && pendingState && router.window) {
          let newState = pendingState;
          let renderPromise = renderDfd.promise;
          let transition2 = router.window.document.startViewTransition(async () => {
            React3.startTransition(() => setStateImpl(newState));
            await renderPromise;
          });
          transition2.finished.finally(() => {
            setRenderDfd(void 0);
            setTransition(void 0);
            setPendingState(void 0);
            setVtContext({ isTransitioning: false });
          });
          setTransition(transition2);
        }
      }, [pendingState, renderDfd, router.window]);
      React3.useEffect(() => {
        if (renderDfd && pendingState && state.location.key === pendingState.location.key) {
          renderDfd.resolve();
        }
      }, [renderDfd, transition, state.location, pendingState]);
      React3.useEffect(() => {
        if (!vtContext.isTransitioning && interruption) {
          setPendingState(interruption.state);
          setVtContext({
            isTransitioning: true,
            flushSync: false,
            currentLocation: interruption.currentLocation,
            nextLocation: interruption.nextLocation
          });
          setInterruption(void 0);
        }
      }, [vtContext.isTransitioning, interruption]);
      let navigator2 = React3.useMemo(() => {
        return {
          createHref: router.createHref,
          encodeLocation: router.encodeLocation,
          go: (n) => router.navigate(n),
          push: (to, state2, opts) => router.navigate(to, {
            state: state2,
            preventScrollReset: opts == null ? void 0 : opts.preventScrollReset
          }),
          replace: (to, state2, opts) => router.navigate(to, {
            replace: true,
            state: state2,
            preventScrollReset: opts == null ? void 0 : opts.preventScrollReset
          })
        };
      }, [router]);
      let basename = router.basename || "/";
      let dataRouterContext = React3.useMemo(
        () => ({
          router,
          navigator: navigator2,
          static: false,
          basename
        }),
        [router, navigator2, basename]
      );
      return React3.createElement(React3.Fragment, null, React3.createElement(DataRouterContext.Provider, { value: dataRouterContext }, React3.createElement(DataRouterStateContext.Provider, { value: state }, React3.createElement(FetchersContext.Provider, { value: fetcherData.current }, React3.createElement(ViewTransitionContext.Provider, { value: vtContext }, React3.createElement(
        Router,
        {
          basename,
          location: state.location,
          navigationType: state.historyAction,
          navigator: navigator2
        },
        React3.createElement(
          MemoizedDataRoutes,
          {
            routes: router.routes,
            future: router.future,
            state
          }
        )
      ))))), null);
    }
    var MemoizedDataRoutes = React3.memo(DataRoutes);
    function DataRoutes({
      routes,
      future,
      state
    }) {
      return useRoutesImpl(routes, void 0, state, future);
    }
    function MemoryRouter({
      basename,
      children,
      initialEntries,
      initialIndex
    }) {
      let historyRef = React3.useRef();
      if (historyRef.current == null) {
        historyRef.current = createMemoryHistory({
          initialEntries,
          initialIndex,
          v5Compat: true
        });
      }
      let history = historyRef.current;
      let [state, setStateImpl] = React3.useState({
        action: history.action,
        location: history.location
      });
      let setState = React3.useCallback(
        (newState) => {
          React3.startTransition(() => setStateImpl(newState));
        },
        [setStateImpl]
      );
      React3.useLayoutEffect(() => history.listen(setState), [history, setState]);
      return React3.createElement(
        Router,
        {
          basename,
          children,
          location: state.location,
          navigationType: state.action,
          navigator: history
        }
      );
    }
    function Navigate({
      to,
      replace: replace2,
      state,
      relative
    }) {
      invariant(
        useInRouterContext(),
        // TODO: This error is probably because they somehow have 2 versions of
        // the router loaded. We can help them understand how to avoid that.
        `<Navigate> may be used only in the context of a <Router> component.`
      );
      let { static: isStatic } = React3.useContext(NavigationContext);
      warning(
        !isStatic,
        `<Navigate> must not be used on the initial render in a <StaticRouter>. This is a no-op, but you should modify your code so the <Navigate> is only ever rendered in response to some user interaction or state change.`
      );
      let { matches } = React3.useContext(RouteContext);
      let { pathname: locationPathname } = useLocation();
      let navigate = useNavigate();
      let path = resolveTo(
        to,
        getResolveToMatches(matches),
        locationPathname,
        relative === "path"
      );
      let jsonPath = JSON.stringify(path);
      React3.useEffect(() => {
        navigate(JSON.parse(jsonPath), { replace: replace2, state, relative });
      }, [navigate, jsonPath, relative, replace2, state]);
      return null;
    }
    function Outlet(props) {
      return useOutlet(props.context);
    }
    function Route(_props) {
      invariant(
        false,
        `A <Route> is only ever to be used as the child of <Routes> element, never rendered directly. Please wrap your <Route> in a <Routes>.`
      );
    }
    function Router({
      basename: basenameProp = "/",
      children = null,
      location: locationProp,
      navigationType = "POP",
      navigator: navigator2,
      static: staticProp = false
    }) {
      invariant(
        !useInRouterContext(),
        `You cannot render a <Router> inside another <Router>. You should never have more than one in your app.`
      );
      let basename = basenameProp.replace(/^\/*/, "/");
      let navigationContext = React3.useMemo(
        () => ({
          basename,
          navigator: navigator2,
          static: staticProp,
          future: {}
        }),
        [basename, navigator2, staticProp]
      );
      if (typeof locationProp === "string") {
        locationProp = parsePath(locationProp);
      }
      let {
        pathname = "/",
        search = "",
        hash = "",
        state = null,
        key = "default"
      } = locationProp;
      let locationContext = React3.useMemo(() => {
        let trailingPathname = stripBasename(pathname, basename);
        if (trailingPathname == null) {
          return null;
        }
        return {
          location: {
            pathname: trailingPathname,
            search,
            hash,
            state,
            key
          },
          navigationType
        };
      }, [basename, pathname, search, hash, state, key, navigationType]);
      warning(
        locationContext != null,
        `<Router basename="${basename}"> is not able to match the URL "${pathname}${search}${hash}" because it does not start with the basename, so the <Router> won't render anything.`
      );
      if (locationContext == null) {
        return null;
      }
      return React3.createElement(NavigationContext.Provider, { value: navigationContext }, React3.createElement(LocationContext.Provider, { children, value: locationContext }));
    }
    function Routes({
      children,
      location
    }) {
      return useRoutes(createRoutesFromChildren(children), location);
    }
    function Await({
      children,
      errorElement,
      resolve
    }) {
      return React3.createElement(AwaitErrorBoundary, { resolve, errorElement }, React3.createElement(ResolveAwait, null, children));
    }
    var AwaitErrorBoundary = class extends React3.Component {
      constructor(props) {
        super(props);
        this.state = { error: null };
      }
      static getDerivedStateFromError(error) {
        return { error };
      }
      componentDidCatch(error, errorInfo) {
        console.error(
          "<Await> caught the following error during render",
          error,
          errorInfo
        );
      }
      render() {
        let { children, errorElement, resolve } = this.props;
        let promise = null;
        let status = 0;
        if (!(resolve instanceof Promise)) {
          status = 1;
          promise = Promise.resolve();
          Object.defineProperty(promise, "_tracked", { get: () => true });
          Object.defineProperty(promise, "_data", { get: () => resolve });
        } else if (this.state.error) {
          status = 2;
          let renderError = this.state.error;
          promise = Promise.reject().catch(() => {
          });
          Object.defineProperty(promise, "_tracked", { get: () => true });
          Object.defineProperty(promise, "_error", { get: () => renderError });
        } else if (resolve._tracked) {
          promise = resolve;
          status = "_error" in promise ? 2 : "_data" in promise ? 1 : 0;
        } else {
          status = 0;
          Object.defineProperty(resolve, "_tracked", { get: () => true });
          promise = resolve.then(
            (data2) => Object.defineProperty(resolve, "_data", { get: () => data2 }),
            (error) => Object.defineProperty(resolve, "_error", { get: () => error })
          );
        }
        if (status === 2 && !errorElement) {
          throw promise._error;
        }
        if (status === 2) {
          return React3.createElement(AwaitContext.Provider, { value: promise, children: errorElement });
        }
        if (status === 1) {
          return React3.createElement(AwaitContext.Provider, { value: promise, children });
        }
        throw promise;
      }
    };
    function ResolveAwait({
      children
    }) {
      let data2 = useAsyncValue();
      let toRender = typeof children === "function" ? children(data2) : children;
      return React3.createElement(React3.Fragment, null, toRender);
    }
    function createRoutesFromChildren(children, parentPath = []) {
      let routes = [];
      React3.Children.forEach(children, (element, index) => {
        if (!React3.isValidElement(element)) {
          return;
        }
        let treePath = [...parentPath, index];
        if (element.type === React3.Fragment) {
          routes.push.apply(
            routes,
            createRoutesFromChildren(element.props.children, treePath)
          );
          return;
        }
        invariant(
          element.type === Route,
          `[${typeof element.type === "string" ? element.type : element.type.name}] is not a <Route> component. All component children of <Routes> must be a <Route> or <React.Fragment>`
        );
        invariant(
          !element.props.index || !element.props.children,
          "An index route cannot have child routes."
        );
        let route = {
          id: element.props.id || treePath.join("-"),
          caseSensitive: element.props.caseSensitive,
          element: element.props.element,
          Component: element.props.Component,
          index: element.props.index,
          path: element.props.path,
          loader: element.props.loader,
          action: element.props.action,
          hydrateFallbackElement: element.props.hydrateFallbackElement,
          HydrateFallback: element.props.HydrateFallback,
          errorElement: element.props.errorElement,
          ErrorBoundary: element.props.ErrorBoundary,
          hasErrorBoundary: element.props.hasErrorBoundary === true || element.props.ErrorBoundary != null || element.props.errorElement != null,
          shouldRevalidate: element.props.shouldRevalidate,
          handle: element.props.handle,
          lazy: element.props.lazy
        };
        if (element.props.children) {
          route.children = createRoutesFromChildren(
            element.props.children,
            treePath
          );
        }
        routes.push(route);
      });
      return routes;
    }
    var createRoutesFromElements = createRoutesFromChildren;
    function renderMatches(matches) {
      return _renderMatches(matches);
    }
    var React10 = __toESM2(require_react());
    var defaultMethod = "get";
    var defaultEncType = "application/x-www-form-urlencoded";
    function isHtmlElement(object) {
      return object != null && typeof object.tagName === "string";
    }
    function isButtonElement(object) {
      return isHtmlElement(object) && object.tagName.toLowerCase() === "button";
    }
    function isFormElement(object) {
      return isHtmlElement(object) && object.tagName.toLowerCase() === "form";
    }
    function isInputElement(object) {
      return isHtmlElement(object) && object.tagName.toLowerCase() === "input";
    }
    function isModifiedEvent(event) {
      return !!(event.metaKey || event.altKey || event.ctrlKey || event.shiftKey);
    }
    function shouldProcessLinkClick(event, target) {
      return event.button === 0 && // Ignore everything but left clicks
      (!target || target === "_self") && // Let browser handle "target=_blank" etc.
      !isModifiedEvent(event);
    }
    function createSearchParams(init = "") {
      return new URLSearchParams(
        typeof init === "string" || Array.isArray(init) || init instanceof URLSearchParams ? init : Object.keys(init).reduce((memo2, key) => {
          let value = init[key];
          return memo2.concat(
            Array.isArray(value) ? value.map((v) => [key, v]) : [[key, value]]
          );
        }, [])
      );
    }
    function getSearchParamsForLocation(locationSearch, defaultSearchParams) {
      let searchParams = createSearchParams(locationSearch);
      if (defaultSearchParams) {
        defaultSearchParams.forEach((_, key) => {
          if (!searchParams.has(key)) {
            defaultSearchParams.getAll(key).forEach((value) => {
              searchParams.append(key, value);
            });
          }
        });
      }
      return searchParams;
    }
    var _formDataSupportsSubmitter = null;
    function isFormDataSubmitterSupported() {
      if (_formDataSupportsSubmitter === null) {
        try {
          new FormData(
            document.createElement("form"),
            // @ts-expect-error if FormData supports the submitter parameter, this will throw
            0
          );
          _formDataSupportsSubmitter = false;
        } catch (e) {
          _formDataSupportsSubmitter = true;
        }
      }
      return _formDataSupportsSubmitter;
    }
    var supportedFormEncTypes = /* @__PURE__ */ new Set([
      "application/x-www-form-urlencoded",
      "multipart/form-data",
      "text/plain"
    ]);
    function getFormEncType(encType) {
      if (encType != null && !supportedFormEncTypes.has(encType)) {
        warning(
          false,
          `"${encType}" is not a valid \`encType\` for \`<Form>\`/\`<fetcher.Form>\` and will default to "${defaultEncType}"`
        );
        return null;
      }
      return encType;
    }
    function getFormSubmissionInfo(target, basename) {
      let method;
      let action;
      let encType;
      let formData;
      let body;
      if (isFormElement(target)) {
        let attr = target.getAttribute("action");
        action = attr ? stripBasename(attr, basename) : null;
        method = target.getAttribute("method") || defaultMethod;
        encType = getFormEncType(target.getAttribute("enctype")) || defaultEncType;
        formData = new FormData(target);
      } else if (isButtonElement(target) || isInputElement(target) && (target.type === "submit" || target.type === "image")) {
        let form = target.form;
        if (form == null) {
          throw new Error(
            `Cannot submit a <button> or <input type="submit"> without a <form>`
          );
        }
        let attr = target.getAttribute("formaction") || form.getAttribute("action");
        action = attr ? stripBasename(attr, basename) : null;
        method = target.getAttribute("formmethod") || form.getAttribute("method") || defaultMethod;
        encType = getFormEncType(target.getAttribute("formenctype")) || getFormEncType(form.getAttribute("enctype")) || defaultEncType;
        formData = new FormData(form, target);
        if (!isFormDataSubmitterSupported()) {
          let { name, type, value } = target;
          if (type === "image") {
            let prefix = name ? `${name}.` : "";
            formData.append(`${prefix}x`, "0");
            formData.append(`${prefix}y`, "0");
          } else if (name) {
            formData.append(name, value);
          }
        }
      } else if (isHtmlElement(target)) {
        throw new Error(
          `Cannot submit element that is not <form>, <button>, or <input type="submit|image">`
        );
      } else {
        method = defaultMethod;
        action = null;
        encType = defaultEncType;
        body = target;
      }
      if (formData && encType === "text/plain") {
        body = formData;
        formData = void 0;
      }
      return { action, method: method.toLowerCase(), encType, formData, body };
    }
    var React9 = __toESM2(require_react());
    function invariant2(value, message) {
      if (value === false || value === null || typeof value === "undefined") {
        throw new Error(message);
      }
    }
    async function loadRouteModule(route, routeModulesCache) {
      if (route.id in routeModulesCache) {
        return routeModulesCache[route.id];
      }
      try {
        let routeModule = await import(
          /* @vite-ignore */
          /* webpackIgnore: true */
          route.module
        );
        routeModulesCache[route.id] = routeModule;
        return routeModule;
      } catch (error) {
        console.error(
          `Error loading route module \`${route.module}\`, reloading page...`
        );
        console.error(error);
        if (window.__reactRouterContext && window.__reactRouterContext.isSpaMode && // @ts-expect-error
        void 0) {
          throw error;
        }
        window.location.reload();
        return new Promise(() => {
        });
      }
    }
    function getKeyedLinksForMatches(matches, routeModules, manifest) {
      let descriptors = matches.map((match) => {
        var _a;
        let module2 = routeModules[match.route.id];
        let route = manifest.routes[match.route.id];
        return [
          route && route.css ? route.css.map((href2) => ({ rel: "stylesheet", href: href2 })) : [],
          ((_a = module2 == null ? void 0 : module2.links) == null ? void 0 : _a.call(module2)) || []
        ];
      }).flat(2);
      let preloads = getModuleLinkHrefs(matches, manifest);
      return dedupeLinkDescriptors(descriptors, preloads);
    }
    function getRouteCssDescriptors(route) {
      if (!route.css) return [];
      return route.css.map((href2) => ({ rel: "stylesheet", href: href2 }));
    }
    async function prefetchRouteCss(route) {
      if (!route.css) return;
      let descriptors = getRouteCssDescriptors(route);
      await Promise.all(descriptors.map(prefetchStyleLink));
    }
    async function prefetchStyleLinks(route, routeModule) {
      if (!route.css && !routeModule.links || !isPreloadSupported()) return;
      let descriptors = [];
      if (route.css) {
        descriptors.push(...getRouteCssDescriptors(route));
      }
      if (routeModule.links) {
        descriptors.push(...routeModule.links());
      }
      if (descriptors.length === 0) return;
      let styleLinks = [];
      for (let descriptor of descriptors) {
        if (!isPageLinkDescriptor(descriptor) && descriptor.rel === "stylesheet") {
          styleLinks.push({
            ...descriptor,
            rel: "preload",
            as: "style"
          });
        }
      }
      await Promise.all(styleLinks.map(prefetchStyleLink));
    }
    async function prefetchStyleLink(descriptor) {
      return new Promise((resolve) => {
        if (descriptor.media && !window.matchMedia(descriptor.media).matches || document.querySelector(
          `link[rel="stylesheet"][href="${descriptor.href}"]`
        )) {
          return resolve();
        }
        let link = document.createElement("link");
        Object.assign(link, descriptor);
        function removeLink() {
          if (document.head.contains(link)) {
            document.head.removeChild(link);
          }
        }
        link.onload = () => {
          removeLink();
          resolve();
        };
        link.onerror = () => {
          removeLink();
          resolve();
        };
        document.head.appendChild(link);
      });
    }
    function isPageLinkDescriptor(object) {
      return object != null && typeof object.page === "string";
    }
    function isHtmlLinkDescriptor(object) {
      if (object == null) {
        return false;
      }
      if (object.href == null) {
        return object.rel === "preload" && typeof object.imageSrcSet === "string" && typeof object.imageSizes === "string";
      }
      return typeof object.rel === "string" && typeof object.href === "string";
    }
    async function getKeyedPrefetchLinks(matches, manifest, routeModules) {
      let links = await Promise.all(
        matches.map(async (match) => {
          let route = manifest.routes[match.route.id];
          if (route) {
            let mod = await loadRouteModule(route, routeModules);
            return mod.links ? mod.links() : [];
          }
          return [];
        })
      );
      return dedupeLinkDescriptors(
        links.flat(1).filter(isHtmlLinkDescriptor).filter((link) => link.rel === "stylesheet" || link.rel === "preload").map(
          (link) => link.rel === "stylesheet" ? { ...link, rel: "prefetch", as: "style" } : { ...link, rel: "prefetch" }
        )
      );
    }
    function getNewMatchesForLinks(page, nextMatches, currentMatches, manifest, location, mode) {
      let isNew = (match, index) => {
        if (!currentMatches[index]) return true;
        return match.route.id !== currentMatches[index].route.id;
      };
      let matchPathChanged = (match, index) => {
        var _a;
        return (
          // param change, /users/123 -> /users/456
          currentMatches[index].pathname !== match.pathname || // splat param changed, which is not present in match.path
          // e.g. /files/images/avatar.jpg -> files/finances.xls
          ((_a = currentMatches[index].route.path) == null ? void 0 : _a.endsWith("*")) && currentMatches[index].params["*"] !== match.params["*"]
        );
      };
      if (mode === "assets") {
        return nextMatches.filter(
          (match, index) => isNew(match, index) || matchPathChanged(match, index)
        );
      }
      if (mode === "data") {
        return nextMatches.filter((match, index) => {
          var _a;
          let manifestRoute = manifest.routes[match.route.id];
          if (!manifestRoute || !manifestRoute.hasLoader) {
            return false;
          }
          if (isNew(match, index) || matchPathChanged(match, index)) {
            return true;
          }
          if (match.route.shouldRevalidate) {
            let routeChoice = match.route.shouldRevalidate({
              currentUrl: new URL(
                location.pathname + location.search + location.hash,
                window.origin
              ),
              currentParams: ((_a = currentMatches[0]) == null ? void 0 : _a.params) || {},
              nextUrl: new URL(page, window.origin),
              nextParams: match.params,
              defaultShouldRevalidate: true
            });
            if (typeof routeChoice === "boolean") {
              return routeChoice;
            }
          }
          return true;
        });
      }
      return [];
    }
    function getModuleLinkHrefs(matches, manifest, { includeHydrateFallback } = {}) {
      return dedupeHrefs(
        matches.map((match) => {
          let route = manifest.routes[match.route.id];
          if (!route) return [];
          let hrefs = [route.module];
          if (route.clientActionModule) {
            hrefs = hrefs.concat(route.clientActionModule);
          }
          if (route.clientLoaderModule) {
            hrefs = hrefs.concat(route.clientLoaderModule);
          }
          if (includeHydrateFallback && route.hydrateFallbackModule) {
            hrefs = hrefs.concat(route.hydrateFallbackModule);
          }
          if (route.imports) {
            hrefs = hrefs.concat(route.imports);
          }
          return hrefs;
        }).flat(1)
      );
    }
    function dedupeHrefs(hrefs) {
      return [...new Set(hrefs)];
    }
    function sortKeys(obj) {
      let sorted = {};
      let keys = Object.keys(obj).sort();
      for (let key of keys) {
        sorted[key] = obj[key];
      }
      return sorted;
    }
    function dedupeLinkDescriptors(descriptors, preloads) {
      let set = /* @__PURE__ */ new Set();
      let preloadsSet = new Set(preloads);
      return descriptors.reduce((deduped, descriptor) => {
        let alreadyModulePreload = preloads && !isPageLinkDescriptor(descriptor) && descriptor.as === "script" && descriptor.href && preloadsSet.has(descriptor.href);
        if (alreadyModulePreload) {
          return deduped;
        }
        let key = JSON.stringify(sortKeys(descriptor));
        if (!set.has(key)) {
          set.add(key);
          deduped.push({ key, link: descriptor });
        }
        return deduped;
      }, []);
    }
    var _isPreloadSupported;
    function isPreloadSupported() {
      if (_isPreloadSupported !== void 0) {
        return _isPreloadSupported;
      }
      let el = document.createElement("link");
      _isPreloadSupported = el.relList.supports("preload");
      el = null;
      return _isPreloadSupported;
    }
    var ESCAPE_LOOKUP = {
      "&": "\\u0026",
      ">": "\\u003e",
      "<": "\\u003c",
      "\u2028": "\\u2028",
      "\u2029": "\\u2029"
    };
    var ESCAPE_REGEX = /[&><\u2028\u2029]/g;
    function escapeHtml(html) {
      return html.replace(ESCAPE_REGEX, (match) => ESCAPE_LOOKUP[match]);
    }
    function createHtml(html) {
      return { __html: html };
    }
    var React4 = __toESM2(require_react());
    var import_turbo_stream = require_turbo_stream();
    async function createRequestInit(request) {
      let init = { signal: request.signal };
      if (request.method !== "GET") {
        init.method = request.method;
        let contentType = request.headers.get("Content-Type");
        if (contentType && /\bapplication\/json\b/.test(contentType)) {
          init.headers = { "Content-Type": contentType };
          init.body = JSON.stringify(await request.json());
        } else if (contentType && /\btext\/plain\b/.test(contentType)) {
          init.headers = { "Content-Type": contentType };
          init.body = await request.text();
        } else if (contentType && /\bapplication\/x-www-form-urlencoded\b/.test(contentType)) {
          init.body = new URLSearchParams(await request.text());
        } else {
          init.body = await request.formData();
        }
      }
      return init;
    }
    var SingleFetchRedirectSymbol = Symbol("SingleFetchRedirect");
    function StreamTransfer({
      context,
      identifier,
      reader,
      textDecoder,
      nonce
    }) {
      if (!context.renderMeta || !context.renderMeta.didRenderScripts) {
        return null;
      }
      if (!context.renderMeta.streamCache) {
        context.renderMeta.streamCache = {};
      }
      let { streamCache } = context.renderMeta;
      let promise = streamCache[identifier];
      if (!promise) {
        promise = streamCache[identifier] = reader.read().then((result) => {
          streamCache[identifier].result = {
            done: result.done,
            value: textDecoder.decode(result.value, { stream: true })
          };
        }).catch((e) => {
          streamCache[identifier].error = e;
        });
      }
      if (promise.error) {
        throw promise.error;
      }
      if (promise.result === void 0) {
        throw promise;
      }
      let { done, value } = promise.result;
      let scriptTag = value ? React4.createElement(
        "script",
        {
          nonce,
          dangerouslySetInnerHTML: {
            __html: `window.__reactRouterContext.streamController.enqueue(${escapeHtml(
              JSON.stringify(value)
            )});`
          }
        }
      ) : null;
      if (done) {
        return React4.createElement(React4.Fragment, null, scriptTag, React4.createElement(
          "script",
          {
            nonce,
            dangerouslySetInnerHTML: {
              __html: `window.__reactRouterContext.streamController.close();`
            }
          }
        ));
      } else {
        return React4.createElement(React4.Fragment, null, scriptTag, React4.createElement(React4.Suspense, null, React4.createElement(
          StreamTransfer,
          {
            context,
            identifier: identifier + 1,
            reader,
            textDecoder,
            nonce
          }
        )));
      }
    }
    function getSingleFetchDataStrategy(manifest, routeModules, ssr, getRouter) {
      return async ({ request, matches, fetcherKey }) => {
        if (request.method !== "GET") {
          return singleFetchActionStrategy(request, matches);
        }
        if (!ssr) {
          let foundRevalidatingServerLoader = matches.some(
            (m) => {
              var _a, _b;
              return m.shouldLoad && ((_a = manifest.routes[m.route.id]) == null ? void 0 : _a.hasLoader) && !((_b = manifest.routes[m.route.id]) == null ? void 0 : _b.hasClientLoader);
            }
          );
          if (!foundRevalidatingServerLoader) {
            let matchesToLoad = matches.filter((m) => m.shouldLoad);
            let url = stripIndexParam(singleFetchUrl(request.url));
            let init = await createRequestInit(request);
            let results = {};
            await Promise.all(
              matchesToLoad.map(
                (m) => m.resolve(async (handler) => {
                  var _a;
                  try {
                    let result = ((_a = manifest.routes[m.route.id]) == null ? void 0 : _a.hasClientLoader) ? await fetchSingleLoader(handler, url, init, m.route.id) : await handler();
                    results[m.route.id] = { type: "data", result };
                  } catch (e) {
                    results[m.route.id] = { type: "error", result: e };
                  }
                })
              )
            );
            return results;
          }
        }
        if (fetcherKey) {
          return singleFetchLoaderFetcherStrategy(request, matches);
        }
        return singleFetchLoaderNavigationStrategy(
          manifest,
          routeModules,
          ssr,
          getRouter(),
          request,
          matches
        );
      };
    }
    async function singleFetchActionStrategy(request, matches) {
      let actionMatch = matches.find((m) => m.shouldLoad);
      invariant2(actionMatch, "No action match found");
      let actionStatus = void 0;
      let result = await actionMatch.resolve(async (handler) => {
        let result2 = await handler(async () => {
          let url = singleFetchUrl(request.url);
          let init = await createRequestInit(request);
          let { data: data2, status } = await fetchAndDecode(url, init);
          actionStatus = status;
          return unwrapSingleFetchResult(
            data2,
            actionMatch.route.id
          );
        });
        return result2;
      });
      if (isResponse(result.result) || isRouteErrorResponse(result.result)) {
        return { [actionMatch.route.id]: result };
      }
      return {
        [actionMatch.route.id]: {
          type: result.type,
          result: data(result.result, actionStatus)
        }
      };
    }
    async function singleFetchLoaderNavigationStrategy(manifest, routeModules, ssr, router, request, matches) {
      let routesParams = /* @__PURE__ */ new Set();
      let foundOptOutRoute = false;
      let routeDfds = matches.map(() => createDeferred2());
      let routesLoadedPromise = Promise.all(routeDfds.map((d) => d.promise));
      let singleFetchDfd = createDeferred2();
      let url = stripIndexParam(singleFetchUrl(request.url));
      let init = await createRequestInit(request);
      let results = {};
      let resolvePromise = Promise.all(
        matches.map(
          async (m, i) => m.resolve(async (handler) => {
            var _a;
            routeDfds[i].resolve();
            let manifestRoute = manifest.routes[m.route.id];
            if (!m.shouldLoad) {
              if (!router.state.initialized) {
                return;
              }
              if (m.route.id in router.state.loaderData && manifestRoute && manifestRoute.hasLoader && ((_a = routeModules[m.route.id]) == null ? void 0 : _a.shouldRevalidate)) {
                foundOptOutRoute = true;
                return;
              }
            }
            if (manifestRoute && manifestRoute.hasClientLoader) {
              if (manifestRoute.hasLoader) {
                foundOptOutRoute = true;
              }
              try {
                let result = await fetchSingleLoader(
                  handler,
                  url,
                  init,
                  m.route.id
                );
                results[m.route.id] = { type: "data", result };
              } catch (e) {
                results[m.route.id] = { type: "error", result: e };
              }
              return;
            }
            if (manifestRoute && manifestRoute.hasLoader) {
              routesParams.add(m.route.id);
            }
            try {
              let result = await handler(async () => {
                let data2 = await singleFetchDfd.promise;
                return unwrapSingleFetchResults(data2, m.route.id);
              });
              results[m.route.id] = {
                type: "data",
                result
              };
            } catch (e) {
              results[m.route.id] = {
                type: "error",
                result: e
              };
            }
          })
        )
      );
      await routesLoadedPromise;
      if ((!router.state.initialized || routesParams.size === 0) && !window.__reactRouterHdrActive) {
        singleFetchDfd.resolve({});
      } else {
        try {
          if (ssr && foundOptOutRoute && routesParams.size > 0) {
            url.searchParams.set(
              "_routes",
              matches.filter((m) => routesParams.has(m.route.id)).map((m) => m.route.id).join(",")
            );
          }
          let data2 = await fetchAndDecode(url, init);
          singleFetchDfd.resolve(data2.data);
        } catch (e) {
          singleFetchDfd.reject(e);
        }
      }
      await resolvePromise;
      return results;
    }
    async function singleFetchLoaderFetcherStrategy(request, matches) {
      let fetcherMatch = matches.find((m) => m.shouldLoad);
      invariant2(fetcherMatch, "No fetcher match found");
      let result = await fetcherMatch.resolve(async (handler) => {
        let url = stripIndexParam(singleFetchUrl(request.url));
        let init = await createRequestInit(request);
        return fetchSingleLoader(handler, url, init, fetcherMatch.route.id);
      });
      return { [fetcherMatch.route.id]: result };
    }
    function fetchSingleLoader(handler, url, init, routeId) {
      return handler(async () => {
        let singleLoaderUrl = new URL(url);
        singleLoaderUrl.searchParams.set("_routes", routeId);
        let { data: data2 } = await fetchAndDecode(singleLoaderUrl, init);
        return unwrapSingleFetchResults(data2, routeId);
      });
    }
    function stripIndexParam(url) {
      let indexValues = url.searchParams.getAll("index");
      url.searchParams.delete("index");
      let indexValuesToKeep = [];
      for (let indexValue of indexValues) {
        if (indexValue) {
          indexValuesToKeep.push(indexValue);
        }
      }
      for (let toKeep of indexValuesToKeep) {
        url.searchParams.append("index", toKeep);
      }
      return url;
    }
    function singleFetchUrl(reqUrl) {
      let url = typeof reqUrl === "string" ? new URL(
        reqUrl,
        // This can be called during the SSR flow via PrefetchPageLinksImpl so
        // don't assume window is available
        typeof window === "undefined" ? "server://singlefetch/" : window.location.origin
      ) : reqUrl;
      if (url.pathname === "/") {
        url.pathname = "_root.data";
      } else {
        url.pathname = `${url.pathname.replace(/\/$/, "")}.data`;
      }
      return url;
    }
    async function fetchAndDecode(url, init) {
      let res = await fetch(url, init);
      if (res.status === 404 && !res.headers.has("X-Remix-Response")) {
        throw new ErrorResponseImpl(404, "Not Found", true);
      }
      const NO_BODY_STATUS_CODES2 = /* @__PURE__ */ new Set([100, 101, 204, 205]);
      if (NO_BODY_STATUS_CODES2.has(res.status)) {
        if (!init.method || init.method === "GET") {
          return { status: res.status, data: {} };
        } else {
          return { status: res.status, data: { data: void 0 } };
        }
      }
      invariant2(res.body, "No response body to decode");
      try {
        let decoded = await decodeViaTurboStream(res.body, window);
        return { status: res.status, data: decoded.value };
      } catch (e) {
        throw new Error("Unable to decode turbo-stream response");
      }
    }
    function decodeViaTurboStream(body, global2) {
      return (0, import_turbo_stream.decode)(body, {
        plugins: [
          (type, ...rest) => {
            if (type === "SanitizedError") {
              let [name, message, stack] = rest;
              let Constructor = Error;
              if (name && name in global2 && typeof global2[name] === "function") {
                Constructor = global2[name];
              }
              let error = new Constructor(message);
              error.stack = stack;
              return { value: error };
            }
            if (type === "ErrorResponse") {
              let [data2, status, statusText] = rest;
              return {
                value: new ErrorResponseImpl(status, statusText, data2)
              };
            }
            if (type === "SingleFetchRedirect") {
              return { value: { [SingleFetchRedirectSymbol]: rest[0] } };
            }
            if (type === "SingleFetchClassInstance") {
              return { value: rest[0] };
            }
            if (type === "SingleFetchFallback") {
              return { value: void 0 };
            }
          }
        ]
      });
    }
    function unwrapSingleFetchResults(results, routeId) {
      let redirect2 = results[SingleFetchRedirectSymbol];
      if (redirect2) {
        return unwrapSingleFetchResult(redirect2, routeId);
      }
      return results[routeId] !== void 0 ? unwrapSingleFetchResult(results[routeId], routeId) : null;
    }
    function unwrapSingleFetchResult(result, routeId) {
      if ("error" in result) {
        throw result.error;
      } else if ("redirect" in result) {
        let headers = {};
        if (result.revalidate) {
          headers["X-Remix-Revalidate"] = "yes";
        }
        if (result.reload) {
          headers["X-Remix-Reload-Document"] = "yes";
        }
        if (result.replace) {
          headers["X-Remix-Replace"] = "yes";
        }
        throw redirect(result.redirect, { status: result.status, headers });
      } else if ("data" in result) {
        return result.data;
      } else {
        throw new Error(`No response found for routeId "${routeId}"`);
      }
    }
    function createDeferred2() {
      let resolve;
      let reject;
      let promise = new Promise((res, rej) => {
        resolve = async (val) => {
          res(val);
          try {
            await promise;
          } catch (e) {
          }
        };
        reject = async (error) => {
          rej(error);
          try {
            await promise;
          } catch (e) {
          }
        };
      });
      return {
        promise,
        //@ts-ignore
        resolve,
        //@ts-ignore
        reject
      };
    }
    var React8 = __toESM2(require_react());
    var React7 = __toESM2(require_react());
    var React5 = __toESM2(require_react());
    var RemixErrorBoundary = class extends React5.Component {
      constructor(props) {
        super(props);
        this.state = { error: props.error || null, location: props.location };
      }
      static getDerivedStateFromError(error) {
        return { error };
      }
      static getDerivedStateFromProps(props, state) {
        if (state.location !== props.location) {
          return { error: props.error || null, location: props.location };
        }
        return { error: props.error || state.error, location: state.location };
      }
      render() {
        if (this.state.error) {
          return React5.createElement(
            RemixRootDefaultErrorBoundary,
            {
              error: this.state.error,
              isOutsideRemixApp: true
            }
          );
        } else {
          return this.props.children;
        }
      }
    };
    function RemixRootDefaultErrorBoundary({
      error,
      isOutsideRemixApp
    }) {
      console.error(error);
      let heyDeveloper = React5.createElement(
        "script",
        {
          dangerouslySetInnerHTML: {
            __html: `
        console.log(
          "💿 Hey developer 👋. You can provide a way better UX than this when your app throws errors. Check out https://remix.run/guides/errors for more information."
        );
      `
          }
        }
      );
      if (isRouteErrorResponse(error)) {
        return React5.createElement(BoundaryShell, { title: "Unhandled Thrown Response!" }, React5.createElement("h1", { style: { fontSize: "24px" } }, error.status, " ", error.statusText), heyDeveloper);
      }
      let errorInstance;
      if (error instanceof Error) {
        errorInstance = error;
      } else {
        let errorString = error == null ? "Unknown Error" : typeof error === "object" && "toString" in error ? error.toString() : JSON.stringify(error);
        errorInstance = new Error(errorString);
      }
      return React5.createElement(
        BoundaryShell,
        {
          title: "Application Error!",
          isOutsideRemixApp
        },
        React5.createElement("h1", { style: { fontSize: "24px" } }, "Application Error"),
        React5.createElement(
          "pre",
          {
            style: {
              padding: "2rem",
              background: "hsla(10, 50%, 50%, 0.1)",
              color: "red",
              overflow: "auto"
            }
          },
          errorInstance.stack
        ),
        heyDeveloper
      );
    }
    function BoundaryShell({
      title,
      renderScripts,
      isOutsideRemixApp,
      children
    }) {
      var _a;
      let { routeModules } = useFrameworkContext();
      if (((_a = routeModules.root) == null ? void 0 : _a.Layout) && !isOutsideRemixApp) {
        return children;
      }
      return React5.createElement("html", { lang: "en" }, React5.createElement("head", null, React5.createElement("meta", { charSet: "utf-8" }), React5.createElement(
        "meta",
        {
          name: "viewport",
          content: "width=device-width,initial-scale=1,viewport-fit=cover"
        }
      ), React5.createElement("title", null, title)), React5.createElement("body", null, React5.createElement("main", { style: { fontFamily: "system-ui, sans-serif", padding: "2rem" } }, children, renderScripts ? React5.createElement(Scripts, null) : null)));
    }
    var React6 = __toESM2(require_react());
    function RemixRootDefaultHydrateFallback() {
      return React6.createElement(BoundaryShell, { title: "Loading...", renderScripts: true }, React6.createElement(
        "script",
        {
          dangerouslySetInnerHTML: {
            __html: `
              console.log(
                "💿 Hey developer 👋. You can provide a way better UX than this " +
                "when your app is loading JS modules and/or running \`clientLoader\` " +
                "functions. Check out https://remix.run/route/hydrate-fallback " +
                "for more information."
              );
            `
          }
        }
      ));
    }
    function groupRoutesByParentId(manifest) {
      let routes = {};
      Object.values(manifest).forEach((route) => {
        if (route) {
          let parentId = route.parentId || "";
          if (!routes[parentId]) {
            routes[parentId] = [];
          }
          routes[parentId].push(route);
        }
      });
      return routes;
    }
    function getRouteComponents(route, routeModule, isSpaMode) {
      let Component4 = getRouteModuleComponent(routeModule);
      let HydrateFallback = routeModule.HydrateFallback && (!isSpaMode || route.id === "root") ? routeModule.HydrateFallback : route.id === "root" ? RemixRootDefaultHydrateFallback : void 0;
      let ErrorBoundary = routeModule.ErrorBoundary ? routeModule.ErrorBoundary : route.id === "root" ? () => React7.createElement(RemixRootDefaultErrorBoundary, { error: useRouteError() }) : void 0;
      if (route.id === "root" && routeModule.Layout) {
        return {
          ...Component4 ? {
            element: React7.createElement(routeModule.Layout, null, React7.createElement(Component4, null))
          } : { Component: Component4 },
          ...ErrorBoundary ? {
            errorElement: React7.createElement(routeModule.Layout, null, React7.createElement(ErrorBoundary, null))
          } : { ErrorBoundary },
          ...HydrateFallback ? {
            hydrateFallbackElement: React7.createElement(routeModule.Layout, null, React7.createElement(HydrateFallback, null))
          } : { HydrateFallback }
        };
      }
      return { Component: Component4, ErrorBoundary, HydrateFallback };
    }
    function createServerRoutes(manifest, routeModules, future, isSpaMode, parentId = "", routesByParentId = groupRoutesByParentId(manifest), spaModeLazyPromise = Promise.resolve({ Component: () => null })) {
      return (routesByParentId[parentId] || []).map((route) => {
        let routeModule = routeModules[route.id];
        invariant2(
          routeModule,
          "No `routeModule` available to create server routes"
        );
        let dataRoute = {
          ...getRouteComponents(route, routeModule, isSpaMode),
          caseSensitive: route.caseSensitive,
          id: route.id,
          index: route.index,
          path: route.path,
          handle: routeModule.handle,
          // For SPA Mode, all routes are lazy except root.  However we tell the
          // router root is also lazy here too since we don't need a full
          // implementation - we just need a `lazy` prop to tell the RR rendering
          // where to stop which is always at the root route in SPA mode
          lazy: isSpaMode ? () => spaModeLazyPromise : void 0,
          // For partial hydration rendering, we need to indicate when the route
          // has a loader/clientLoader, but it won't ever be called during the static
          // render, so just give it a no-op function so we can render down to the
          // proper fallback
          loader: route.hasLoader || route.hasClientLoader ? () => null : void 0
          // We don't need action/shouldRevalidate on these routes since they're
          // for a static render
        };
        let children = createServerRoutes(
          manifest,
          routeModules,
          future,
          isSpaMode,
          route.id,
          routesByParentId,
          spaModeLazyPromise
        );
        if (children.length > 0) dataRoute.children = children;
        return dataRoute;
      });
    }
    function createClientRoutesWithHMRRevalidationOptOut(needsRevalidation, manifest, routeModulesCache, initialState, ssr, isSpaMode) {
      return createClientRoutes(
        manifest,
        routeModulesCache,
        initialState,
        ssr,
        isSpaMode,
        "",
        groupRoutesByParentId(manifest),
        needsRevalidation
      );
    }
    function preventInvalidServerHandlerCall(type, route) {
      if (type === "loader" && !route.hasLoader || type === "action" && !route.hasAction) {
        let fn = type === "action" ? "serverAction()" : "serverLoader()";
        let msg = `You are trying to call ${fn} on a route that does not have a server ${type} (routeId: "${route.id}")`;
        console.error(msg);
        throw new ErrorResponseImpl(400, "Bad Request", new Error(msg), true);
      }
    }
    function noActionDefinedError(type, routeId) {
      let article = type === "clientAction" ? "a" : "an";
      let msg = `Route "${routeId}" does not have ${article} ${type}, but you are trying to submit to it. To fix this, please add ${article} \`${type}\` function to the route`;
      console.error(msg);
      throw new ErrorResponseImpl(405, "Method Not Allowed", new Error(msg), true);
    }
    function createClientRoutes(manifest, routeModulesCache, initialState, ssr, isSpaMode, parentId = "", routesByParentId = groupRoutesByParentId(manifest), needsRevalidation) {
      return (routesByParentId[parentId] || []).map((route) => {
        var _a, _b, _c;
        let routeModule = routeModulesCache[route.id];
        function fetchServerHandler(singleFetch) {
          invariant2(
            typeof singleFetch === "function",
            "No single fetch function available for route handler"
          );
          return singleFetch();
        }
        function fetchServerLoader(singleFetch) {
          if (!route.hasLoader) return Promise.resolve(null);
          return fetchServerHandler(singleFetch);
        }
        function fetchServerAction(singleFetch) {
          if (!route.hasAction) {
            throw noActionDefinedError("action", route.id);
          }
          return fetchServerHandler(singleFetch);
        }
        function prefetchModule(modulePath) {
          import(
            /* @vite-ignore */
            /* webpackIgnore: true */
            modulePath
          );
        }
        function prefetchRouteModuleChunks(route2) {
          if (route2.clientActionModule) {
            prefetchModule(route2.clientActionModule);
          }
          if (route2.clientLoaderModule) {
            prefetchModule(route2.clientLoaderModule);
          }
        }
        async function prefetchStylesAndCallHandler(handler) {
          let cachedModule = routeModulesCache[route.id];
          let linkPrefetchPromise = cachedModule ? prefetchStyleLinks(route, cachedModule) : Promise.resolve();
          try {
            return handler();
          } finally {
            await linkPrefetchPromise;
          }
        }
        let dataRoute = {
          id: route.id,
          index: route.index,
          path: route.path
        };
        if (routeModule) {
          Object.assign(dataRoute, {
            ...dataRoute,
            ...getRouteComponents(route, routeModule, isSpaMode),
            handle: routeModule.handle,
            shouldRevalidate: getShouldRevalidateFunction(
              routeModule,
              route,
              ssr,
              needsRevalidation
            )
          });
          let hasInitialData = initialState && initialState.loaderData && route.id in initialState.loaderData;
          let initialData = hasInitialData ? (_a = initialState == null ? void 0 : initialState.loaderData) == null ? void 0 : _a[route.id] : void 0;
          let hasInitialError = initialState && initialState.errors && route.id in initialState.errors;
          let initialError = hasInitialError ? (_b = initialState == null ? void 0 : initialState.errors) == null ? void 0 : _b[route.id] : void 0;
          let isHydrationRequest = needsRevalidation == null && (((_c = routeModule.clientLoader) == null ? void 0 : _c.hydrate) === true || !route.hasLoader);
          dataRoute.loader = async ({ request, params }, singleFetch) => {
            try {
              let result = await prefetchStylesAndCallHandler(async () => {
                invariant2(
                  routeModule,
                  "No `routeModule` available for critical-route loader"
                );
                if (!routeModule.clientLoader) {
                  return fetchServerLoader(singleFetch);
                }
                return routeModule.clientLoader({
                  request,
                  params,
                  async serverLoader() {
                    preventInvalidServerHandlerCall("loader", route);
                    if (isHydrationRequest) {
                      if (hasInitialData) {
                        return initialData;
                      }
                      if (hasInitialError) {
                        throw initialError;
                      }
                    }
                    return fetchServerLoader(singleFetch);
                  }
                });
              });
              return result;
            } finally {
              isHydrationRequest = false;
            }
          };
          dataRoute.loader.hydrate = shouldHydrateRouteLoader(
            route,
            routeModule,
            isSpaMode
          );
          dataRoute.action = ({ request, params }, singleFetch) => {
            return prefetchStylesAndCallHandler(async () => {
              invariant2(
                routeModule,
                "No `routeModule` available for critical-route action"
              );
              if (!routeModule.clientAction) {
                if (isSpaMode) {
                  throw noActionDefinedError("clientAction", route.id);
                }
                return fetchServerAction(singleFetch);
              }
              return routeModule.clientAction({
                request,
                params,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            });
          };
        } else {
          if (!route.hasClientLoader) {
            dataRoute.loader = ({ request }, singleFetch) => prefetchStylesAndCallHandler(() => {
              return fetchServerLoader(singleFetch);
            });
          } else if (route.clientLoaderModule) {
            dataRoute.loader = async (args, singleFetch) => {
              invariant2(route.clientLoaderModule);
              let { clientLoader } = await import(
                /* @vite-ignore */
                /* webpackIgnore: true */
                route.clientLoaderModule
              );
              return clientLoader({
                ...args,
                async serverLoader() {
                  preventInvalidServerHandlerCall("loader", route);
                  return fetchServerLoader(singleFetch);
                }
              });
            };
          }
          if (!route.hasClientAction) {
            dataRoute.action = ({ request }, singleFetch) => prefetchStylesAndCallHandler(() => {
              if (isSpaMode) {
                throw noActionDefinedError("clientAction", route.id);
              }
              return fetchServerAction(singleFetch);
            });
          } else if (route.clientActionModule) {
            dataRoute.action = async (args, singleFetch) => {
              invariant2(route.clientActionModule);
              prefetchRouteModuleChunks(route);
              let { clientAction } = await import(
                /* @vite-ignore */
                /* webpackIgnore: true */
                route.clientActionModule
              );
              return clientAction({
                ...args,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            };
          }
          dataRoute.lazy = async () => {
            if (route.clientLoaderModule || route.clientActionModule) {
              await new Promise((resolve) => setTimeout(resolve, 0));
            }
            let modPromise = loadRouteModuleWithBlockingLinks(
              route,
              routeModulesCache
            );
            prefetchRouteModuleChunks(route);
            let mod = await modPromise;
            let lazyRoute = { ...mod };
            if (mod.clientLoader) {
              let clientLoader = mod.clientLoader;
              lazyRoute.loader = (args, singleFetch) => clientLoader({
                ...args,
                async serverLoader() {
                  preventInvalidServerHandlerCall("loader", route);
                  return fetchServerLoader(singleFetch);
                }
              });
            }
            if (mod.clientAction) {
              let clientAction = mod.clientAction;
              lazyRoute.action = (args, singleFetch) => clientAction({
                ...args,
                async serverAction() {
                  preventInvalidServerHandlerCall("action", route);
                  return fetchServerAction(singleFetch);
                }
              });
            }
            return {
              ...lazyRoute.loader ? { loader: lazyRoute.loader } : {},
              ...lazyRoute.action ? { action: lazyRoute.action } : {},
              hasErrorBoundary: lazyRoute.hasErrorBoundary,
              shouldRevalidate: getShouldRevalidateFunction(
                lazyRoute,
                route,
                ssr,
                needsRevalidation
              ),
              handle: lazyRoute.handle,
              // No need to wrap these in layout since the root route is never
              // loaded via route.lazy()
              Component: lazyRoute.Component,
              ErrorBoundary: lazyRoute.ErrorBoundary
            };
          };
        }
        let children = createClientRoutes(
          manifest,
          routeModulesCache,
          initialState,
          ssr,
          isSpaMode,
          route.id,
          routesByParentId,
          needsRevalidation
        );
        if (children.length > 0) dataRoute.children = children;
        return dataRoute;
      });
    }
    function getShouldRevalidateFunction(route, manifestRoute, ssr, needsRevalidation) {
      if (needsRevalidation) {
        return wrapShouldRevalidateForHdr(
          manifestRoute.id,
          route.shouldRevalidate,
          needsRevalidation
        );
      }
      if (!ssr && manifestRoute.hasLoader && !manifestRoute.hasClientLoader) {
        if (route.shouldRevalidate) {
          let fn = route.shouldRevalidate;
          return (opts) => fn({ ...opts, defaultShouldRevalidate: false });
        } else {
          return () => false;
        }
      }
      if (ssr && route.shouldRevalidate) {
        let fn = route.shouldRevalidate;
        return (opts) => fn({ ...opts, defaultShouldRevalidate: true });
      }
      return route.shouldRevalidate;
    }
    function wrapShouldRevalidateForHdr(routeId, routeShouldRevalidate, needsRevalidation) {
      let handledRevalidation = false;
      return (arg) => {
        if (!handledRevalidation) {
          handledRevalidation = true;
          return needsRevalidation.has(routeId);
        }
        return routeShouldRevalidate ? routeShouldRevalidate(arg) : arg.defaultShouldRevalidate;
      };
    }
    async function loadRouteModuleWithBlockingLinks(route, routeModules) {
      let routeModulePromise = loadRouteModule(route, routeModules);
      let prefetchRouteCssPromise = prefetchRouteCss(route);
      let routeModule = await routeModulePromise;
      await Promise.all([
        prefetchRouteCssPromise,
        prefetchStyleLinks(route, routeModule)
      ]);
      return {
        Component: getRouteModuleComponent(routeModule),
        ErrorBoundary: routeModule.ErrorBoundary,
        clientAction: routeModule.clientAction,
        clientLoader: routeModule.clientLoader,
        handle: routeModule.handle,
        links: routeModule.links,
        meta: routeModule.meta,
        shouldRevalidate: routeModule.shouldRevalidate
      };
    }
    function getRouteModuleComponent(routeModule) {
      if (routeModule.default == null) return void 0;
      let isEmptyObject = typeof routeModule.default === "object" && Object.keys(routeModule.default).length === 0;
      if (!isEmptyObject) {
        return routeModule.default;
      }
    }
    function shouldHydrateRouteLoader(route, routeModule, isSpaMode) {
      return isSpaMode && route.id !== "root" || routeModule.clientLoader != null && (routeModule.clientLoader.hydrate === true || route.hasLoader !== true);
    }
    var nextPaths = /* @__PURE__ */ new Set();
    var discoveredPathsMaxSize = 1e3;
    var discoveredPaths = /* @__PURE__ */ new Set();
    var URL_LIMIT = 7680;
    function isFogOfWarEnabled(ssr) {
      return ssr === true;
    }
    function getPartialManifest(manifest, router) {
      let routeIds = new Set(router.state.matches.map((m) => m.route.id));
      let segments = router.state.location.pathname.split("/").filter(Boolean);
      let paths = ["/"];
      segments.pop();
      while (segments.length > 0) {
        paths.push(`/${segments.join("/")}`);
        segments.pop();
      }
      paths.forEach((path) => {
        let matches = matchRoutes(router.routes, path, router.basename);
        if (matches) {
          matches.forEach((m) => routeIds.add(m.route.id));
        }
      });
      let initialRoutes = [...routeIds].reduce(
        (acc, id) => Object.assign(acc, { [id]: manifest.routes[id] }),
        {}
      );
      return {
        ...manifest,
        routes: initialRoutes
      };
    }
    function getPatchRoutesOnNavigationFunction(manifest, routeModules, ssr, isSpaMode, basename) {
      if (!isFogOfWarEnabled(ssr)) {
        return void 0;
      }
      return async ({ path, patch, signal }) => {
        if (discoveredPaths.has(path)) {
          return;
        }
        await fetchAndApplyManifestPatches(
          [path],
          manifest,
          routeModules,
          ssr,
          isSpaMode,
          basename,
          patch,
          signal
        );
      };
    }
    function useFogOFWarDiscovery(router, manifest, routeModules, ssr, isSpaMode) {
      React8.useEffect(() => {
        var _a;
        if (!isFogOfWarEnabled(ssr) || ((_a = navigator.connection) == null ? void 0 : _a.saveData) === true) {
          return;
        }
        function registerElement(el) {
          let path = el.tagName === "FORM" ? el.getAttribute("action") : el.getAttribute("href");
          if (!path) {
            return;
          }
          let pathname = el.tagName === "A" ? el.pathname : new URL(path, window.location.origin).pathname;
          if (!discoveredPaths.has(pathname)) {
            nextPaths.add(pathname);
          }
        }
        async function fetchPatches() {
          document.querySelectorAll("a[data-discover], form[data-discover]").forEach(registerElement);
          let lazyPaths = Array.from(nextPaths.keys()).filter((path) => {
            if (discoveredPaths.has(path)) {
              nextPaths.delete(path);
              return false;
            }
            return true;
          });
          if (lazyPaths.length === 0) {
            return;
          }
          try {
            await fetchAndApplyManifestPatches(
              lazyPaths,
              manifest,
              routeModules,
              ssr,
              isSpaMode,
              router.basename,
              router.patchRoutes
            );
          } catch (e) {
            console.error("Failed to fetch manifest patches", e);
          }
        }
        let debouncedFetchPatches = debounce(fetchPatches, 100);
        fetchPatches();
        let observer = new MutationObserver(() => debouncedFetchPatches());
        observer.observe(document.documentElement, {
          subtree: true,
          childList: true,
          attributes: true,
          attributeFilter: ["data-discover", "href", "action"]
        });
        return () => observer.disconnect();
      }, [ssr, isSpaMode, manifest, routeModules, router]);
    }
    async function fetchAndApplyManifestPatches(paths, manifest, routeModules, ssr, isSpaMode, basename, patchRoutes, signal) {
      let manifestPath = `${basename != null ? basename : "/"}/__manifest`.replace(
        /\/+/g,
        "/"
      );
      let url = new URL(manifestPath, window.location.origin);
      paths.sort().forEach((path) => url.searchParams.append("p", path));
      url.searchParams.set("version", manifest.version);
      if (url.toString().length > URL_LIMIT) {
        nextPaths.clear();
        return;
      }
      let serverPatches;
      try {
        let res = await fetch(url, { signal });
        if (!res.ok) {
          throw new Error(`${res.status} ${res.statusText}`);
        } else if (res.status >= 400) {
          throw new Error(await res.text());
        }
        serverPatches = await res.json();
      } catch (e) {
        if (signal == null ? void 0 : signal.aborted) return;
        throw e;
      }
      let knownRoutes = new Set(Object.keys(manifest.routes));
      let patches = Object.values(serverPatches).reduce((acc, route) => {
        if (route && !knownRoutes.has(route.id)) {
          acc[route.id] = route;
        }
        return acc;
      }, {});
      Object.assign(manifest.routes, patches);
      paths.forEach((p) => addToFifoQueue(p, discoveredPaths));
      let parentIds = /* @__PURE__ */ new Set();
      Object.values(patches).forEach((patch) => {
        if (patch && (!patch.parentId || !patches[patch.parentId])) {
          parentIds.add(patch.parentId);
        }
      });
      parentIds.forEach(
        (parentId) => patchRoutes(
          parentId || null,
          createClientRoutes(patches, routeModules, null, ssr, isSpaMode, parentId)
        )
      );
    }
    function addToFifoQueue(path, queue) {
      if (queue.size >= discoveredPathsMaxSize) {
        let first = queue.values().next().value;
        queue.delete(first);
      }
      queue.add(path);
    }
    function debounce(callback, wait) {
      let timeoutId;
      return (...args) => {
        window.clearTimeout(timeoutId);
        timeoutId = window.setTimeout(() => callback(...args), wait);
      };
    }
    function useDataRouterContext2() {
      let context = React9.useContext(DataRouterContext);
      invariant2(
        context,
        "You must render this element inside a <DataRouterContext.Provider> element"
      );
      return context;
    }
    function useDataRouterStateContext() {
      let context = React9.useContext(DataRouterStateContext);
      invariant2(
        context,
        "You must render this element inside a <DataRouterStateContext.Provider> element"
      );
      return context;
    }
    var FrameworkContext = React9.createContext(void 0);
    FrameworkContext.displayName = "FrameworkContext";
    function useFrameworkContext() {
      let context = React9.useContext(FrameworkContext);
      invariant2(
        context,
        "You must render this element inside a <HydratedRouter> element"
      );
      return context;
    }
    function usePrefetchBehavior(prefetch, theirElementProps) {
      let frameworkContext = React9.useContext(FrameworkContext);
      let [maybePrefetch, setMaybePrefetch] = React9.useState(false);
      let [shouldPrefetch, setShouldPrefetch] = React9.useState(false);
      let { onFocus, onBlur, onMouseEnter, onMouseLeave, onTouchStart } = theirElementProps;
      let ref = React9.useRef(null);
      React9.useEffect(() => {
        if (prefetch === "render") {
          setShouldPrefetch(true);
        }
        if (prefetch === "viewport") {
          let callback = (entries) => {
            entries.forEach((entry) => {
              setShouldPrefetch(entry.isIntersecting);
            });
          };
          let observer = new IntersectionObserver(callback, { threshold: 0.5 });
          if (ref.current) observer.observe(ref.current);
          return () => {
            observer.disconnect();
          };
        }
      }, [prefetch]);
      React9.useEffect(() => {
        if (maybePrefetch) {
          let id = setTimeout(() => {
            setShouldPrefetch(true);
          }, 100);
          return () => {
            clearTimeout(id);
          };
        }
      }, [maybePrefetch]);
      let setIntent = () => {
        setMaybePrefetch(true);
      };
      let cancelIntent = () => {
        setMaybePrefetch(false);
        setShouldPrefetch(false);
      };
      if (!frameworkContext) {
        return [false, ref, {}];
      }
      if (prefetch !== "intent") {
        return [shouldPrefetch, ref, {}];
      }
      return [
        shouldPrefetch,
        ref,
        {
          onFocus: composeEventHandlers(onFocus, setIntent),
          onBlur: composeEventHandlers(onBlur, cancelIntent),
          onMouseEnter: composeEventHandlers(onMouseEnter, setIntent),
          onMouseLeave: composeEventHandlers(onMouseLeave, cancelIntent),
          onTouchStart: composeEventHandlers(onTouchStart, setIntent)
        }
      ];
    }
    function composeEventHandlers(theirHandler, ourHandler) {
      return (event) => {
        theirHandler && theirHandler(event);
        if (!event.defaultPrevented) {
          ourHandler(event);
        }
      };
    }
    function getActiveMatches(matches, errors, isSpaMode) {
      if (isSpaMode && !isHydrated) {
        return [matches[0]];
      }
      if (errors) {
        let errorIdx = matches.findIndex((m) => errors[m.route.id] !== void 0);
        return matches.slice(0, errorIdx + 1);
      }
      return matches;
    }
    function Links() {
      let { isSpaMode, manifest, routeModules, criticalCss } = useFrameworkContext();
      let { errors, matches: routerMatches } = useDataRouterStateContext();
      let matches = getActiveMatches(routerMatches, errors, isSpaMode);
      let keyedLinks = React9.useMemo(
        () => getKeyedLinksForMatches(matches, routeModules, manifest),
        [matches, routeModules, manifest]
      );
      return React9.createElement(React9.Fragment, null, criticalCss ? React9.createElement("style", { dangerouslySetInnerHTML: { __html: criticalCss } }) : null, keyedLinks.map(
        ({ key, link }) => isPageLinkDescriptor(link) ? React9.createElement(PrefetchPageLinks, { key, ...link }) : React9.createElement("link", { key, ...link })
      ));
    }
    function PrefetchPageLinks({
      page,
      ...dataLinkProps
    }) {
      let { router } = useDataRouterContext2();
      let matches = React9.useMemo(
        () => matchRoutes(router.routes, page, router.basename),
        [router.routes, page, router.basename]
      );
      if (!matches) {
        return null;
      }
      return React9.createElement(PrefetchPageLinksImpl, { page, matches, ...dataLinkProps });
    }
    function useKeyedPrefetchLinks(matches) {
      let { manifest, routeModules } = useFrameworkContext();
      let [keyedPrefetchLinks, setKeyedPrefetchLinks] = React9.useState([]);
      React9.useEffect(() => {
        let interrupted = false;
        void getKeyedPrefetchLinks(matches, manifest, routeModules).then(
          (links) => {
            if (!interrupted) {
              setKeyedPrefetchLinks(links);
            }
          }
        );
        return () => {
          interrupted = true;
        };
      }, [matches, manifest, routeModules]);
      return keyedPrefetchLinks;
    }
    function PrefetchPageLinksImpl({
      page,
      matches: nextMatches,
      ...linkProps
    }) {
      let location = useLocation();
      let { manifest, routeModules } = useFrameworkContext();
      let { loaderData, matches } = useDataRouterStateContext();
      let newMatchesForData = React9.useMemo(
        () => getNewMatchesForLinks(
          page,
          nextMatches,
          matches,
          manifest,
          location,
          "data"
        ),
        [page, nextMatches, matches, manifest, location]
      );
      let newMatchesForAssets = React9.useMemo(
        () => getNewMatchesForLinks(
          page,
          nextMatches,
          matches,
          manifest,
          location,
          "assets"
        ),
        [page, nextMatches, matches, manifest, location]
      );
      let dataHrefs = React9.useMemo(() => {
        if (page === location.pathname + location.search + location.hash) {
          return [];
        }
        let routesParams = /* @__PURE__ */ new Set();
        let foundOptOutRoute = false;
        nextMatches.forEach((m) => {
          var _a;
          let manifestRoute = manifest.routes[m.route.id];
          if (!manifestRoute || !manifestRoute.hasLoader) {
            return;
          }
          if (!newMatchesForData.some((m2) => m2.route.id === m.route.id) && m.route.id in loaderData && ((_a = routeModules[m.route.id]) == null ? void 0 : _a.shouldRevalidate)) {
            foundOptOutRoute = true;
          } else if (manifestRoute.hasClientLoader) {
            foundOptOutRoute = true;
          } else {
            routesParams.add(m.route.id);
          }
        });
        if (routesParams.size === 0) {
          return [];
        }
        let url = singleFetchUrl(page);
        if (foundOptOutRoute && routesParams.size > 0) {
          url.searchParams.set(
            "_routes",
            nextMatches.filter((m) => routesParams.has(m.route.id)).map((m) => m.route.id).join(",")
          );
        }
        return [url.pathname + url.search];
      }, [
        loaderData,
        location,
        manifest,
        newMatchesForData,
        nextMatches,
        page,
        routeModules
      ]);
      let moduleHrefs = React9.useMemo(
        () => getModuleLinkHrefs(newMatchesForAssets, manifest),
        [newMatchesForAssets, manifest]
      );
      let keyedPrefetchLinks = useKeyedPrefetchLinks(newMatchesForAssets);
      return React9.createElement(React9.Fragment, null, dataHrefs.map((href2) => React9.createElement("link", { key: href2, rel: "prefetch", as: "fetch", href: href2, ...linkProps })), moduleHrefs.map((href2) => React9.createElement("link", { key: href2, rel: "modulepreload", href: href2, ...linkProps })), keyedPrefetchLinks.map(({ key, link }) => (
        // these don't spread `linkProps` because they are full link descriptors
        // already with their own props
        React9.createElement("link", { key, ...link })
      )));
    }
    function Meta() {
      let { isSpaMode, routeModules } = useFrameworkContext();
      let {
        errors,
        matches: routerMatches,
        loaderData
      } = useDataRouterStateContext();
      let location = useLocation();
      let _matches = getActiveMatches(routerMatches, errors, isSpaMode);
      let error = null;
      if (errors) {
        error = errors[_matches[_matches.length - 1].route.id];
      }
      let meta = [];
      let leafMeta = null;
      let matches = [];
      for (let i = 0; i < _matches.length; i++) {
        let _match = _matches[i];
        let routeId = _match.route.id;
        let data2 = loaderData[routeId];
        let params = _match.params;
        let routeModule = routeModules[routeId];
        let routeMeta = [];
        let match = {
          id: routeId,
          data: data2,
          meta: [],
          params: _match.params,
          pathname: _match.pathname,
          handle: _match.route.handle,
          error
        };
        matches[i] = match;
        if (routeModule == null ? void 0 : routeModule.meta) {
          routeMeta = typeof routeModule.meta === "function" ? routeModule.meta({
            data: data2,
            params,
            location,
            matches,
            error
          }) : Array.isArray(routeModule.meta) ? [...routeModule.meta] : routeModule.meta;
        } else if (leafMeta) {
          routeMeta = [...leafMeta];
        }
        routeMeta = routeMeta || [];
        if (!Array.isArray(routeMeta)) {
          throw new Error(
            "The route at " + _match.route.path + " returns an invalid value. All route meta functions must return an array of meta objects.\n\nTo reference the meta function API, see https://remix.run/route/meta"
          );
        }
        match.meta = routeMeta;
        matches[i] = match;
        meta = [...routeMeta];
        leafMeta = meta;
      }
      return React9.createElement(React9.Fragment, null, meta.flat().map((metaProps) => {
        if (!metaProps) {
          return null;
        }
        if ("tagName" in metaProps) {
          let { tagName, ...rest } = metaProps;
          if (!isValidMetaTag(tagName)) {
            console.warn(
              `A meta object uses an invalid tagName: ${tagName}. Expected either 'link' or 'meta'`
            );
            return null;
          }
          let Comp = tagName;
          return React9.createElement(Comp, { key: JSON.stringify(rest), ...rest });
        }
        if ("title" in metaProps) {
          return React9.createElement("title", { key: "title" }, String(metaProps.title));
        }
        if ("charset" in metaProps) {
          metaProps.charSet ?? (metaProps.charSet = metaProps.charset);
          delete metaProps.charset;
        }
        if ("charSet" in metaProps && metaProps.charSet != null) {
          return typeof metaProps.charSet === "string" ? React9.createElement("meta", { key: "charSet", charSet: metaProps.charSet }) : null;
        }
        if ("script:ld+json" in metaProps) {
          try {
            let json = JSON.stringify(metaProps["script:ld+json"]);
            return React9.createElement(
              "script",
              {
                key: `script:ld+json:${json}`,
                type: "application/ld+json",
                dangerouslySetInnerHTML: { __html: json }
              }
            );
          } catch (err) {
            return null;
          }
        }
        return React9.createElement("meta", { key: JSON.stringify(metaProps), ...metaProps });
      }));
    }
    function isValidMetaTag(tagName) {
      return typeof tagName === "string" && /^(meta|link)$/.test(tagName);
    }
    var isHydrated = false;
    function Scripts(props) {
      let { manifest, serverHandoffString, isSpaMode, ssr, renderMeta } = useFrameworkContext();
      let { router, static: isStatic, staticContext } = useDataRouterContext2();
      let { matches: routerMatches } = useDataRouterStateContext();
      let enableFogOfWar = isFogOfWarEnabled(ssr);
      if (renderMeta) {
        renderMeta.didRenderScripts = true;
      }
      let matches = getActiveMatches(routerMatches, null, isSpaMode);
      React9.useEffect(() => {
        isHydrated = true;
      }, []);
      let initialScripts = React9.useMemo(() => {
        var _a;
        let streamScript = "window.__reactRouterContext.stream = new ReadableStream({start(controller){window.__reactRouterContext.streamController = controller;}}).pipeThrough(new TextEncoderStream());";
        let contextScript = staticContext ? `window.__reactRouterContext = ${serverHandoffString};${streamScript}` : " ";
        let routeModulesScript = !isStatic ? " " : `${((_a = manifest.hmr) == null ? void 0 : _a.runtime) ? `import ${JSON.stringify(manifest.hmr.runtime)};` : ""}${!enableFogOfWar ? `import ${JSON.stringify(manifest.url)}` : ""};
${matches.map((match, routeIndex) => {
          let routeVarName = `route${routeIndex}`;
          let manifestEntry = manifest.routes[match.route.id];
          invariant2(manifestEntry, `Route ${match.route.id} not found in manifest`);
          let {
            clientActionModule,
            clientLoaderModule,
            hydrateFallbackModule,
            module: module2
          } = manifestEntry;
          let chunks = [
            ...clientActionModule ? [
              {
                module: clientActionModule,
                varName: `${routeVarName}_clientAction`
              }
            ] : [],
            ...clientLoaderModule ? [
              {
                module: clientLoaderModule,
                varName: `${routeVarName}_clientLoader`
              }
            ] : [],
            ...hydrateFallbackModule ? [
              {
                module: hydrateFallbackModule,
                varName: `${routeVarName}_HydrateFallback`
              }
            ] : [],
            { module: module2, varName: `${routeVarName}_main` }
          ];
          if (chunks.length === 1) {
            return `import * as ${routeVarName} from ${JSON.stringify(module2)};`;
          }
          let chunkImportsSnippet = chunks.map((chunk) => `import * as ${chunk.varName} from "${chunk.module}";`).join("\n");
          let mergedChunksSnippet = `const ${routeVarName} = {${chunks.map((chunk) => `...${chunk.varName}`).join(",")}};`;
          return [chunkImportsSnippet, mergedChunksSnippet].join("\n");
        }).join("\n")}
  ${enableFogOfWar ? (
          // Inline a minimal manifest with the SSR matches
          `window.__reactRouterManifest = ${JSON.stringify(
            getPartialManifest(manifest, router),
            null,
            2
          )};`
        ) : ""}
  window.__reactRouterRouteModules = {${matches.map((match, index) => `${JSON.stringify(match.route.id)}:route${index}`).join(",")}};

import(${JSON.stringify(manifest.entry.module)});`;
        return React9.createElement(React9.Fragment, null, React9.createElement(
          "script",
          {
            ...props,
            suppressHydrationWarning: true,
            dangerouslySetInnerHTML: createHtml(contextScript),
            type: void 0
          }
        ), React9.createElement(
          "script",
          {
            ...props,
            suppressHydrationWarning: true,
            dangerouslySetInnerHTML: createHtml(routeModulesScript),
            type: "module",
            async: true
          }
        ));
      }, []);
      let preloads = isHydrated ? [] : manifest.entry.imports.concat(
        getModuleLinkHrefs(matches, manifest, {
          includeHydrateFallback: true
        })
      );
      return isHydrated ? null : React9.createElement(React9.Fragment, null, !enableFogOfWar ? React9.createElement(
        "link",
        {
          rel: "modulepreload",
          href: manifest.url,
          crossOrigin: props.crossOrigin
        }
      ) : null, React9.createElement(
        "link",
        {
          rel: "modulepreload",
          href: manifest.entry.module,
          crossOrigin: props.crossOrigin
        }
      ), dedupe(preloads).map((path) => React9.createElement(
        "link",
        {
          key: path,
          rel: "modulepreload",
          href: path,
          crossOrigin: props.crossOrigin
        }
      )), initialScripts);
    }
    function dedupe(array) {
      return [...new Set(array)];
    }
    function mergeRefs(...refs) {
      return (value) => {
        refs.forEach((ref) => {
          if (typeof ref === "function") {
            ref(value);
          } else if (ref != null) {
            ref.current = value;
          }
        });
      };
    }
    var isBrowser = typeof window !== "undefined" && typeof window.document !== "undefined" && typeof window.document.createElement !== "undefined";
    try {
      if (isBrowser) {
        window.__reactRouterVersion = "7.2.0";
      }
    } catch (e) {
    }
    function createBrowserRouter(routes, opts) {
      return createRouter({
        basename: opts == null ? void 0 : opts.basename,
        future: opts == null ? void 0 : opts.future,
        history: createBrowserHistory({ window: opts == null ? void 0 : opts.window }),
        hydrationData: (opts == null ? void 0 : opts.hydrationData) || parseHydrationData(),
        routes,
        mapRouteProperties,
        dataStrategy: opts == null ? void 0 : opts.dataStrategy,
        patchRoutesOnNavigation: opts == null ? void 0 : opts.patchRoutesOnNavigation,
        window: opts == null ? void 0 : opts.window
      }).initialize();
    }
    function createHashRouter(routes, opts) {
      return createRouter({
        basename: opts == null ? void 0 : opts.basename,
        future: opts == null ? void 0 : opts.future,
        history: createHashHistory({ window: opts == null ? void 0 : opts.window }),
        hydrationData: (opts == null ? void 0 : opts.hydrationData) || parseHydrationData(),
        routes,
        mapRouteProperties,
        dataStrategy: opts == null ? void 0 : opts.dataStrategy,
        patchRoutesOnNavigation: opts == null ? void 0 : opts.patchRoutesOnNavigation,
        window: opts == null ? void 0 : opts.window
      }).initialize();
    }
    function parseHydrationData() {
      let state = window == null ? void 0 : window.__staticRouterHydrationData;
      if (state && state.errors) {
        state = {
          ...state,
          errors: deserializeErrors(state.errors)
        };
      }
      return state;
    }
    function deserializeErrors(errors) {
      if (!errors) return null;
      let entries = Object.entries(errors);
      let serialized = {};
      for (let [key, val] of entries) {
        if (val && val.__type === "RouteErrorResponse") {
          serialized[key] = new ErrorResponseImpl(
            val.status,
            val.statusText,
            val.data,
            val.internal === true
          );
        } else if (val && val.__type === "Error") {
          if (val.__subType) {
            let ErrorConstructor = window[val.__subType];
            if (typeof ErrorConstructor === "function") {
              try {
                let error = new ErrorConstructor(val.message);
                error.stack = "";
                serialized[key] = error;
              } catch (e) {
              }
            }
          }
          if (serialized[key] == null) {
            let error = new Error(val.message);
            error.stack = "";
            serialized[key] = error;
          }
        } else {
          serialized[key] = val;
        }
      }
      return serialized;
    }
    function BrowserRouter({
      basename,
      children,
      window: window2
    }) {
      let historyRef = React10.useRef();
      if (historyRef.current == null) {
        historyRef.current = createBrowserHistory({ window: window2, v5Compat: true });
      }
      let history = historyRef.current;
      let [state, setStateImpl] = React10.useState({
        action: history.action,
        location: history.location
      });
      let setState = React10.useCallback(
        (newState) => {
          React10.startTransition(() => setStateImpl(newState));
        },
        [setStateImpl]
      );
      React10.useLayoutEffect(() => history.listen(setState), [history, setState]);
      return React10.createElement(
        Router,
        {
          basename,
          children,
          location: state.location,
          navigationType: state.action,
          navigator: history
        }
      );
    }
    function HashRouter({ basename, children, window: window2 }) {
      let historyRef = React10.useRef();
      if (historyRef.current == null) {
        historyRef.current = createHashHistory({ window: window2, v5Compat: true });
      }
      let history = historyRef.current;
      let [state, setStateImpl] = React10.useState({
        action: history.action,
        location: history.location
      });
      let setState = React10.useCallback(
        (newState) => {
          React10.startTransition(() => setStateImpl(newState));
        },
        [setStateImpl]
      );
      React10.useLayoutEffect(() => history.listen(setState), [history, setState]);
      return React10.createElement(
        Router,
        {
          basename,
          children,
          location: state.location,
          navigationType: state.action,
          navigator: history
        }
      );
    }
    function HistoryRouter({
      basename,
      children,
      history
    }) {
      let [state, setStateImpl] = React10.useState({
        action: history.action,
        location: history.location
      });
      let setState = React10.useCallback(
        (newState) => {
          React10.startTransition(() => setStateImpl(newState));
        },
        [setStateImpl]
      );
      React10.useLayoutEffect(() => history.listen(setState), [history, setState]);
      return React10.createElement(
        Router,
        {
          basename,
          children,
          location: state.location,
          navigationType: state.action,
          navigator: history
        }
      );
    }
    HistoryRouter.displayName = "unstable_HistoryRouter";
    var ABSOLUTE_URL_REGEX2 = /^(?:[a-z][a-z0-9+.-]*:|\/\/)/i;
    var Link = React10.forwardRef(
      function LinkWithRef({
        onClick,
        discover = "render",
        prefetch = "none",
        relative,
        reloadDocument,
        replace: replace2,
        state,
        target,
        to,
        preventScrollReset,
        viewTransition,
        ...rest
      }, forwardedRef) {
        let { basename } = React10.useContext(NavigationContext);
        let isAbsolute = typeof to === "string" && ABSOLUTE_URL_REGEX2.test(to);
        let absoluteHref;
        let isExternal = false;
        if (typeof to === "string" && isAbsolute) {
          absoluteHref = to;
          if (isBrowser) {
            try {
              let currentUrl = new URL(window.location.href);
              let targetUrl = to.startsWith("//") ? new URL(currentUrl.protocol + to) : new URL(to);
              let path = stripBasename(targetUrl.pathname, basename);
              if (targetUrl.origin === currentUrl.origin && path != null) {
                to = path + targetUrl.search + targetUrl.hash;
              } else {
                isExternal = true;
              }
            } catch (e) {
              warning(
                false,
                `<Link to="${to}"> contains an invalid URL which will probably break when clicked - please update to a valid URL path.`
              );
            }
          }
        }
        let href2 = useHref(to, { relative });
        let [shouldPrefetch, prefetchRef, prefetchHandlers] = usePrefetchBehavior(
          prefetch,
          rest
        );
        let internalOnClick = useLinkClickHandler(to, {
          replace: replace2,
          state,
          target,
          preventScrollReset,
          relative,
          viewTransition
        });
        function handleClick(event) {
          if (onClick) onClick(event);
          if (!event.defaultPrevented) {
            internalOnClick(event);
          }
        }
        let link = (
          // eslint-disable-next-line jsx-a11y/anchor-has-content
          React10.createElement(
            "a",
            {
              ...rest,
              ...prefetchHandlers,
              href: absoluteHref || href2,
              onClick: isExternal || reloadDocument ? onClick : handleClick,
              ref: mergeRefs(forwardedRef, prefetchRef),
              target,
              "data-discover": !isAbsolute && discover === "render" ? "true" : void 0
            }
          )
        );
        return shouldPrefetch && !isAbsolute ? React10.createElement(React10.Fragment, null, link, React10.createElement(PrefetchPageLinks, { page: href2 })) : link;
      }
    );
    Link.displayName = "Link";
    var NavLink = React10.forwardRef(
      function NavLinkWithRef({
        "aria-current": ariaCurrentProp = "page",
        caseSensitive = false,
        className: classNameProp = "",
        end = false,
        style: styleProp,
        to,
        viewTransition,
        children,
        ...rest
      }, ref) {
        let path = useResolvedPath(to, { relative: rest.relative });
        let location = useLocation();
        let routerState = React10.useContext(DataRouterStateContext);
        let { navigator: navigator2, basename } = React10.useContext(NavigationContext);
        let isTransitioning = routerState != null && // Conditional usage is OK here because the usage of a data router is static
        // eslint-disable-next-line react-hooks/rules-of-hooks
        useViewTransitionState(path) && viewTransition === true;
        let toPathname = navigator2.encodeLocation ? navigator2.encodeLocation(path).pathname : path.pathname;
        let locationPathname = location.pathname;
        let nextLocationPathname = routerState && routerState.navigation && routerState.navigation.location ? routerState.navigation.location.pathname : null;
        if (!caseSensitive) {
          locationPathname = locationPathname.toLowerCase();
          nextLocationPathname = nextLocationPathname ? nextLocationPathname.toLowerCase() : null;
          toPathname = toPathname.toLowerCase();
        }
        if (nextLocationPathname && basename) {
          nextLocationPathname = stripBasename(nextLocationPathname, basename) || nextLocationPathname;
        }
        const endSlashPosition = toPathname !== "/" && toPathname.endsWith("/") ? toPathname.length - 1 : toPathname.length;
        let isActive = locationPathname === toPathname || !end && locationPathname.startsWith(toPathname) && locationPathname.charAt(endSlashPosition) === "/";
        let isPending = nextLocationPathname != null && (nextLocationPathname === toPathname || !end && nextLocationPathname.startsWith(toPathname) && nextLocationPathname.charAt(toPathname.length) === "/");
        let renderProps = {
          isActive,
          isPending,
          isTransitioning
        };
        let ariaCurrent = isActive ? ariaCurrentProp : void 0;
        let className;
        if (typeof classNameProp === "function") {
          className = classNameProp(renderProps);
        } else {
          className = [
            classNameProp,
            isActive ? "active" : null,
            isPending ? "pending" : null,
            isTransitioning ? "transitioning" : null
          ].filter(Boolean).join(" ");
        }
        let style = typeof styleProp === "function" ? styleProp(renderProps) : styleProp;
        return React10.createElement(
          Link,
          {
            ...rest,
            "aria-current": ariaCurrent,
            className,
            ref,
            style,
            to,
            viewTransition
          },
          typeof children === "function" ? children(renderProps) : children
        );
      }
    );
    NavLink.displayName = "NavLink";
    var Form = React10.forwardRef(
      ({
        discover = "render",
        fetcherKey,
        navigate,
        reloadDocument,
        replace: replace2,
        state,
        method = defaultMethod,
        action,
        onSubmit,
        relative,
        preventScrollReset,
        viewTransition,
        ...props
      }, forwardedRef) => {
        let submit = useSubmit();
        let formAction = useFormAction(action, { relative });
        let formMethod = method.toLowerCase() === "get" ? "get" : "post";
        let isAbsolute = typeof action === "string" && ABSOLUTE_URL_REGEX2.test(action);
        let submitHandler = (event) => {
          onSubmit && onSubmit(event);
          if (event.defaultPrevented) return;
          event.preventDefault();
          let submitter = event.nativeEvent.submitter;
          let submitMethod = (submitter == null ? void 0 : submitter.getAttribute("formmethod")) || method;
          submit(submitter || event.currentTarget, {
            fetcherKey,
            method: submitMethod,
            navigate,
            replace: replace2,
            state,
            relative,
            preventScrollReset,
            viewTransition
          });
        };
        return React10.createElement(
          "form",
          {
            ref: forwardedRef,
            method: formMethod,
            action: formAction,
            onSubmit: reloadDocument ? onSubmit : submitHandler,
            ...props,
            "data-discover": !isAbsolute && discover === "render" ? "true" : void 0
          }
        );
      }
    );
    Form.displayName = "Form";
    function ScrollRestoration({
      getKey,
      storageKey,
      ...props
    }) {
      let remixContext = React10.useContext(FrameworkContext);
      let { basename } = React10.useContext(NavigationContext);
      let location = useLocation();
      let matches = useMatches();
      useScrollRestoration({ getKey, storageKey });
      let ssrKey = React10.useMemo(
        () => {
          if (!remixContext || !getKey) return null;
          let userKey = getScrollRestorationKey(
            location,
            matches,
            basename,
            getKey
          );
          return userKey !== location.key ? userKey : null;
        },
        // Nah, we only need this the first time for the SSR render
        // eslint-disable-next-line react-hooks/exhaustive-deps
        []
      );
      if (!remixContext || remixContext.isSpaMode) {
        return null;
      }
      let restoreScroll = ((storageKey2, restoreKey) => {
        if (!window.history.state || !window.history.state.key) {
          let key = Math.random().toString(32).slice(2);
          window.history.replaceState({ key }, "");
        }
        try {
          let positions = JSON.parse(sessionStorage.getItem(storageKey2) || "{}");
          let storedY = positions[restoreKey || window.history.state.key];
          if (typeof storedY === "number") {
            window.scrollTo(0, storedY);
          }
        } catch (error) {
          console.error(error);
          sessionStorage.removeItem(storageKey2);
        }
      }).toString();
      return React10.createElement(
        "script",
        {
          ...props,
          suppressHydrationWarning: true,
          dangerouslySetInnerHTML: {
            __html: `(${restoreScroll})(${JSON.stringify(
              storageKey || SCROLL_RESTORATION_STORAGE_KEY
            )}, ${JSON.stringify(ssrKey)})`
          }
        }
      );
    }
    ScrollRestoration.displayName = "ScrollRestoration";
    function getDataRouterConsoleError2(hookName) {
      return `${hookName} must be used within a data router.  See https://reactrouter.com/en/main/routers/picking-a-router.`;
    }
    function useDataRouterContext3(hookName) {
      let ctx = React10.useContext(DataRouterContext);
      invariant(ctx, getDataRouterConsoleError2(hookName));
      return ctx;
    }
    function useDataRouterState2(hookName) {
      let state = React10.useContext(DataRouterStateContext);
      invariant(state, getDataRouterConsoleError2(hookName));
      return state;
    }
    function useLinkClickHandler(to, {
      target,
      replace: replaceProp,
      state,
      preventScrollReset,
      relative,
      viewTransition
    } = {}) {
      let navigate = useNavigate();
      let location = useLocation();
      let path = useResolvedPath(to, { relative });
      return React10.useCallback(
        (event) => {
          if (shouldProcessLinkClick(event, target)) {
            event.preventDefault();
            let replace2 = replaceProp !== void 0 ? replaceProp : createPath(location) === createPath(path);
            navigate(to, {
              replace: replace2,
              state,
              preventScrollReset,
              relative,
              viewTransition
            });
          }
        },
        [
          location,
          navigate,
          path,
          replaceProp,
          state,
          target,
          to,
          preventScrollReset,
          relative,
          viewTransition
        ]
      );
    }
    function useSearchParams(defaultInit) {
      warning(
        typeof URLSearchParams !== "undefined",
        `You cannot use the \`useSearchParams\` hook in a browser that does not support the URLSearchParams API. If you need to support Internet Explorer 11, we recommend you load a polyfill such as https://github.com/ungap/url-search-params.`
      );
      let defaultSearchParamsRef = React10.useRef(createSearchParams(defaultInit));
      let hasSetSearchParamsRef = React10.useRef(false);
      let location = useLocation();
      let searchParams = React10.useMemo(
        () => (
          // Only merge in the defaults if we haven't yet called setSearchParams.
          // Once we call that we want those to take precedence, otherwise you can't
          // remove a param with setSearchParams({}) if it has an initial value
          getSearchParamsForLocation(
            location.search,
            hasSetSearchParamsRef.current ? null : defaultSearchParamsRef.current
          )
        ),
        [location.search]
      );
      let navigate = useNavigate();
      let setSearchParams = React10.useCallback(
        (nextInit, navigateOptions) => {
          const newSearchParams = createSearchParams(
            typeof nextInit === "function" ? nextInit(searchParams) : nextInit
          );
          hasSetSearchParamsRef.current = true;
          navigate("?" + newSearchParams, navigateOptions);
        },
        [navigate, searchParams]
      );
      return [searchParams, setSearchParams];
    }
    var fetcherId = 0;
    var getUniqueFetcherId = () => `__${String(++fetcherId)}__`;
    function useSubmit() {
      let { router } = useDataRouterContext3(
        "useSubmit"
        /* UseSubmit */
      );
      let { basename } = React10.useContext(NavigationContext);
      let currentRouteId = useRouteId();
      return React10.useCallback(
        async (target, options = {}) => {
          let { action, method, encType, formData, body } = getFormSubmissionInfo(
            target,
            basename
          );
          if (options.navigate === false) {
            let key = options.fetcherKey || getUniqueFetcherId();
            await router.fetch(key, currentRouteId, options.action || action, {
              preventScrollReset: options.preventScrollReset,
              formData,
              body,
              formMethod: options.method || method,
              formEncType: options.encType || encType,
              flushSync: options.flushSync
            });
          } else {
            await router.navigate(options.action || action, {
              preventScrollReset: options.preventScrollReset,
              formData,
              body,
              formMethod: options.method || method,
              formEncType: options.encType || encType,
              replace: options.replace,
              state: options.state,
              fromRouteId: currentRouteId,
              flushSync: options.flushSync,
              viewTransition: options.viewTransition
            });
          }
        },
        [router, basename, currentRouteId]
      );
    }
    function useFormAction(action, { relative } = {}) {
      let { basename } = React10.useContext(NavigationContext);
      let routeContext = React10.useContext(RouteContext);
      invariant(routeContext, "useFormAction must be used inside a RouteContext");
      let [match] = routeContext.matches.slice(-1);
      let path = { ...useResolvedPath(action ? action : ".", { relative }) };
      let location = useLocation();
      if (action == null) {
        path.search = location.search;
        let params = new URLSearchParams(path.search);
        let indexValues = params.getAll("index");
        let hasNakedIndexParam = indexValues.some((v) => v === "");
        if (hasNakedIndexParam) {
          params.delete("index");
          indexValues.filter((v) => v).forEach((v) => params.append("index", v));
          let qs = params.toString();
          path.search = qs ? `?${qs}` : "";
        }
      }
      if ((!action || action === ".") && match.route.index) {
        path.search = path.search ? path.search.replace(/^\?/, "?index&") : "?index";
      }
      if (basename !== "/") {
        path.pathname = path.pathname === "/" ? basename : joinPaths([basename, path.pathname]);
      }
      return createPath(path);
    }
    function useFetcher({
      key
    } = {}) {
      var _a;
      let { router } = useDataRouterContext3(
        "useFetcher"
        /* UseFetcher */
      );
      let state = useDataRouterState2(
        "useFetcher"
        /* UseFetcher */
      );
      let fetcherData = React10.useContext(FetchersContext);
      let route = React10.useContext(RouteContext);
      let routeId = (_a = route.matches[route.matches.length - 1]) == null ? void 0 : _a.route.id;
      invariant(fetcherData, `useFetcher must be used inside a FetchersContext`);
      invariant(route, `useFetcher must be used inside a RouteContext`);
      invariant(
        routeId != null,
        `useFetcher can only be used on routes that contain a unique "id"`
      );
      let defaultKey = React10.useId();
      let [fetcherKey, setFetcherKey] = React10.useState(key || defaultKey);
      if (key && key !== fetcherKey) {
        setFetcherKey(key);
      }
      React10.useEffect(() => {
        router.getFetcher(fetcherKey);
        return () => router.deleteFetcher(fetcherKey);
      }, [router, fetcherKey]);
      let load = React10.useCallback(
        async (href2, opts) => {
          invariant(routeId, "No routeId available for fetcher.load()");
          await router.fetch(fetcherKey, routeId, href2, opts);
        },
        [fetcherKey, routeId, router]
      );
      let submitImpl = useSubmit();
      let submit = React10.useCallback(
        async (target, opts) => {
          await submitImpl(target, {
            ...opts,
            navigate: false,
            fetcherKey
          });
        },
        [fetcherKey, submitImpl]
      );
      let FetcherForm = React10.useMemo(() => {
        let FetcherForm2 = React10.forwardRef(
          (props, ref) => {
            return React10.createElement(Form, { ...props, navigate: false, fetcherKey, ref });
          }
        );
        FetcherForm2.displayName = "fetcher.Form";
        return FetcherForm2;
      }, [fetcherKey]);
      let fetcher = state.fetchers.get(fetcherKey) || IDLE_FETCHER;
      let data2 = fetcherData.get(fetcherKey);
      let fetcherWithComponents = React10.useMemo(
        () => ({
          Form: FetcherForm,
          submit,
          load,
          ...fetcher,
          data: data2
        }),
        [FetcherForm, submit, load, fetcher, data2]
      );
      return fetcherWithComponents;
    }
    function useFetchers() {
      let state = useDataRouterState2(
        "useFetchers"
        /* UseFetchers */
      );
      return Array.from(state.fetchers.entries()).map(([key, fetcher]) => ({
        ...fetcher,
        key
      }));
    }
    var SCROLL_RESTORATION_STORAGE_KEY = "react-router-scroll-positions";
    var savedScrollPositions = {};
    function getScrollRestorationKey(location, matches, basename, getKey) {
      let key = null;
      if (getKey) {
        if (basename !== "/") {
          key = getKey(
            {
              ...location,
              pathname: stripBasename(location.pathname, basename) || location.pathname
            },
            matches
          );
        } else {
          key = getKey(location, matches);
        }
      }
      if (key == null) {
        key = location.key;
      }
      return key;
    }
    function useScrollRestoration({
      getKey,
      storageKey
    } = {}) {
      let { router } = useDataRouterContext3(
        "useScrollRestoration"
        /* UseScrollRestoration */
      );
      let { restoreScrollPosition, preventScrollReset } = useDataRouterState2(
        "useScrollRestoration"
        /* UseScrollRestoration */
      );
      let { basename } = React10.useContext(NavigationContext);
      let location = useLocation();
      let matches = useMatches();
      let navigation = useNavigation();
      React10.useEffect(() => {
        window.history.scrollRestoration = "manual";
        return () => {
          window.history.scrollRestoration = "auto";
        };
      }, []);
      usePageHide(
        React10.useCallback(() => {
          if (navigation.state === "idle") {
            let key = getScrollRestorationKey(location, matches, basename, getKey);
            savedScrollPositions[key] = window.scrollY;
          }
          try {
            sessionStorage.setItem(
              storageKey || SCROLL_RESTORATION_STORAGE_KEY,
              JSON.stringify(savedScrollPositions)
            );
          } catch (error) {
            warning(
              false,
              `Failed to save scroll positions in sessionStorage, <ScrollRestoration /> will not work properly (${error}).`
            );
          }
          window.history.scrollRestoration = "auto";
        }, [navigation.state, getKey, basename, location, matches, storageKey])
      );
      if (typeof document !== "undefined") {
        React10.useLayoutEffect(() => {
          try {
            let sessionPositions = sessionStorage.getItem(
              storageKey || SCROLL_RESTORATION_STORAGE_KEY
            );
            if (sessionPositions) {
              savedScrollPositions = JSON.parse(sessionPositions);
            }
          } catch (e) {
          }
        }, [storageKey]);
        React10.useLayoutEffect(() => {
          let disableScrollRestoration = router == null ? void 0 : router.enableScrollRestoration(
            savedScrollPositions,
            () => window.scrollY,
            getKey ? (location2, matches2) => getScrollRestorationKey(location2, matches2, basename, getKey) : void 0
          );
          return () => disableScrollRestoration && disableScrollRestoration();
        }, [router, basename, getKey]);
        React10.useLayoutEffect(() => {
          if (restoreScrollPosition === false) {
            return;
          }
          if (typeof restoreScrollPosition === "number") {
            window.scrollTo(0, restoreScrollPosition);
            return;
          }
          if (location.hash) {
            let el = document.getElementById(
              decodeURIComponent(location.hash.slice(1))
            );
            if (el) {
              el.scrollIntoView();
              return;
            }
          }
          if (preventScrollReset === true) {
            return;
          }
          window.scrollTo(0, 0);
        }, [location, restoreScrollPosition, preventScrollReset]);
      }
    }
    function useBeforeUnload(callback, options) {
      let { capture } = options || {};
      React10.useEffect(() => {
        let opts = capture != null ? { capture } : void 0;
        window.addEventListener("beforeunload", callback, opts);
        return () => {
          window.removeEventListener("beforeunload", callback, opts);
        };
      }, [callback, capture]);
    }
    function usePageHide(callback, options) {
      let { capture } = options || {};
      React10.useEffect(() => {
        let opts = capture != null ? { capture } : void 0;
        window.addEventListener("pagehide", callback, opts);
        return () => {
          window.removeEventListener("pagehide", callback, opts);
        };
      }, [callback, capture]);
    }
    function usePrompt({
      when,
      message
    }) {
      let blocker = useBlocker(when);
      React10.useEffect(() => {
        if (blocker.state === "blocked") {
          let proceed = window.confirm(message);
          if (proceed) {
            setTimeout(blocker.proceed, 0);
          } else {
            blocker.reset();
          }
        }
      }, [blocker, message]);
      React10.useEffect(() => {
        if (blocker.state === "blocked" && !when) {
          blocker.reset();
        }
      }, [blocker, when]);
    }
    function useViewTransitionState(to, opts = {}) {
      let vtContext = React10.useContext(ViewTransitionContext);
      invariant(
        vtContext != null,
        "`useViewTransitionState` must be used within `react-router-dom`'s `RouterProvider`.  Did you accidentally import `RouterProvider` from `react-router`?"
      );
      let { basename } = useDataRouterContext3(
        "useViewTransitionState"
        /* useViewTransitionState */
      );
      let path = useResolvedPath(to, { relative: opts.relative });
      if (!vtContext.isTransitioning) {
        return false;
      }
      let currentPath = stripBasename(vtContext.currentLocation.pathname, basename) || vtContext.currentLocation.pathname;
      let nextPath = stripBasename(vtContext.nextLocation.pathname, basename) || vtContext.nextLocation.pathname;
      return matchPath(path.pathname, nextPath) != null || matchPath(path.pathname, currentPath) != null;
    }
    var React11 = __toESM2(require_react());
    function StaticRouter({
      basename,
      children,
      location: locationProp = "/"
    }) {
      if (typeof locationProp === "string") {
        locationProp = parsePath(locationProp);
      }
      let action = "POP";
      let location = {
        pathname: locationProp.pathname || "/",
        search: locationProp.search || "",
        hash: locationProp.hash || "",
        state: locationProp.state != null ? locationProp.state : null,
        key: locationProp.key || "default"
      };
      let staticNavigator = getStatelessNavigator();
      return React11.createElement(
        Router,
        {
          basename,
          children,
          location,
          navigationType: action,
          navigator: staticNavigator,
          static: true
        }
      );
    }
    function StaticRouterProvider({
      context,
      router,
      hydrate = true,
      nonce
    }) {
      invariant(
        router && context,
        "You must provide `router` and `context` to <StaticRouterProvider>"
      );
      let dataRouterContext = {
        router,
        navigator: getStatelessNavigator(),
        static: true,
        staticContext: context,
        basename: context.basename || "/"
      };
      let fetchersContext = /* @__PURE__ */ new Map();
      let hydrateScript = "";
      if (hydrate !== false) {
        let data2 = {
          loaderData: context.loaderData,
          actionData: context.actionData,
          errors: serializeErrors(context.errors)
        };
        let json = htmlEscape(JSON.stringify(JSON.stringify(data2)));
        hydrateScript = `window.__staticRouterHydrationData = JSON.parse(${json});`;
      }
      let { state } = dataRouterContext.router;
      return React11.createElement(React11.Fragment, null, React11.createElement(DataRouterContext.Provider, { value: dataRouterContext }, React11.createElement(DataRouterStateContext.Provider, { value: state }, React11.createElement(FetchersContext.Provider, { value: fetchersContext }, React11.createElement(ViewTransitionContext.Provider, { value: { isTransitioning: false } }, React11.createElement(
        Router,
        {
          basename: dataRouterContext.basename,
          location: state.location,
          navigationType: state.historyAction,
          navigator: dataRouterContext.navigator,
          static: dataRouterContext.static
        },
        React11.createElement(
          DataRoutes2,
          {
            routes: router.routes,
            future: router.future,
            state
          }
        )
      ))))), hydrateScript ? React11.createElement(
        "script",
        {
          suppressHydrationWarning: true,
          nonce,
          dangerouslySetInnerHTML: { __html: hydrateScript }
        }
      ) : null);
    }
    function DataRoutes2({
      routes,
      future,
      state
    }) {
      return useRoutesImpl(routes, void 0, state, future);
    }
    function serializeErrors(errors) {
      if (!errors) return null;
      let entries = Object.entries(errors);
      let serialized = {};
      for (let [key, val] of entries) {
        if (isRouteErrorResponse(val)) {
          serialized[key] = { ...val, __type: "RouteErrorResponse" };
        } else if (val instanceof Error) {
          serialized[key] = {
            message: val.message,
            __type: "Error",
            // If this is a subclass (i.e., ReferenceError), send up the type so we
            // can re-create the same type during hydration.
            ...val.name !== "Error" ? {
              __subType: val.name
            } : {}
          };
        } else {
          serialized[key] = val;
        }
      }
      return serialized;
    }
    function getStatelessNavigator() {
      return {
        createHref,
        encodeLocation,
        push(to) {
          throw new Error(
            `You cannot use navigator.push() on the server because it is a stateless environment. This error was probably triggered when you did a \`navigate(${JSON.stringify(to)})\` somewhere in your app.`
          );
        },
        replace(to) {
          throw new Error(
            `You cannot use navigator.replace() on the server because it is a stateless environment. This error was probably triggered when you did a \`navigate(${JSON.stringify(to)}, { replace: true })\` somewhere in your app.`
          );
        },
        go(delta) {
          throw new Error(
            `You cannot use navigator.go() on the server because it is a stateless environment. This error was probably triggered when you did a \`navigate(${delta})\` somewhere in your app.`
          );
        },
        back() {
          throw new Error(
            `You cannot use navigator.back() on the server because it is a stateless environment.`
          );
        },
        forward() {
          throw new Error(
            `You cannot use navigator.forward() on the server because it is a stateless environment.`
          );
        }
      };
    }
    function createStaticHandler2(routes, opts) {
      return createStaticHandler(routes, {
        ...opts,
        mapRouteProperties
      });
    }
    function createStaticRouter(routes, context, opts = {}) {
      let manifest = {};
      let dataRoutes = convertRoutesToDataRoutes(
        routes,
        mapRouteProperties,
        void 0,
        manifest
      );
      let matches = context.matches.map((match) => {
        let route = manifest[match.route.id] || match.route;
        return {
          ...match,
          route
        };
      });
      let msg = (method) => `You cannot use router.${method}() on the server because it is a stateless environment`;
      return {
        get basename() {
          return context.basename;
        },
        get future() {
          return {
            ...opts == null ? void 0 : opts.future
          };
        },
        get state() {
          return {
            historyAction: "POP",
            location: context.location,
            matches,
            loaderData: context.loaderData,
            actionData: context.actionData,
            errors: context.errors,
            initialized: true,
            navigation: IDLE_NAVIGATION,
            restoreScrollPosition: null,
            preventScrollReset: false,
            revalidation: "idle",
            fetchers: /* @__PURE__ */ new Map(),
            blockers: /* @__PURE__ */ new Map()
          };
        },
        get routes() {
          return dataRoutes;
        },
        get window() {
          return void 0;
        },
        initialize() {
          throw msg("initialize");
        },
        subscribe() {
          throw msg("subscribe");
        },
        enableScrollRestoration() {
          throw msg("enableScrollRestoration");
        },
        navigate() {
          throw msg("navigate");
        },
        fetch() {
          throw msg("fetch");
        },
        revalidate() {
          throw msg("revalidate");
        },
        createHref,
        encodeLocation,
        getFetcher() {
          return IDLE_FETCHER;
        },
        deleteFetcher() {
          throw msg("deleteFetcher");
        },
        dispose() {
          throw msg("dispose");
        },
        getBlocker() {
          return IDLE_BLOCKER;
        },
        deleteBlocker() {
          throw msg("deleteBlocker");
        },
        patchRoutes() {
          throw msg("patchRoutes");
        },
        _internalFetchControllers: /* @__PURE__ */ new Map(),
        _internalSetRoutes() {
          throw msg("_internalSetRoutes");
        }
      };
    }
    function createHref(to) {
      return typeof to === "string" ? to : createPath(to);
    }
    function encodeLocation(to) {
      let href2 = typeof to === "string" ? to : createPath(to);
      href2 = href2.replace(/ $/, "%20");
      let encoded = ABSOLUTE_URL_REGEX3.test(href2) ? new URL(href2) : new URL(href2, "http://localhost");
      return {
        pathname: encoded.pathname,
        search: encoded.search,
        hash: encoded.hash
      };
    }
    var ABSOLUTE_URL_REGEX3 = /^(?:[a-z][a-z0-9+.-]*:|\/\/)/i;
    var ESCAPE_LOOKUP2 = {
      "&": "\\u0026",
      ">": "\\u003e",
      "<": "\\u003c",
      "\u2028": "\\u2028",
      "\u2029": "\\u2029"
    };
    var ESCAPE_REGEX2 = /[&><\u2028\u2029]/g;
    function htmlEscape(str) {
      return str.replace(ESCAPE_REGEX2, (match) => ESCAPE_LOOKUP2[match]);
    }
    var React12 = __toESM2(require_react());
    function ServerRouter({
      context,
      url,
      nonce
    }) {
      if (typeof url === "string") {
        url = new URL(url);
      }
      let { manifest, routeModules, criticalCss, serverHandoffString } = context;
      let routes = createServerRoutes(
        manifest.routes,
        routeModules,
        context.future,
        context.isSpaMode
      );
      context.staticHandlerContext.loaderData = {
        ...context.staticHandlerContext.loaderData
      };
      for (let match of context.staticHandlerContext.matches) {
        let routeId = match.route.id;
        let route = routeModules[routeId];
        let manifestRoute = context.manifest.routes[routeId];
        if (route && manifestRoute && shouldHydrateRouteLoader(manifestRoute, route, context.isSpaMode) && (route.HydrateFallback || !manifestRoute.hasLoader)) {
          delete context.staticHandlerContext.loaderData[routeId];
        }
      }
      let router = createStaticRouter(routes, context.staticHandlerContext);
      return React12.createElement(React12.Fragment, null, React12.createElement(
        FrameworkContext.Provider,
        {
          value: {
            manifest,
            routeModules,
            criticalCss,
            serverHandoffString,
            future: context.future,
            ssr: context.ssr,
            isSpaMode: context.isSpaMode,
            serializeError: context.serializeError,
            renderMeta: context.renderMeta
          }
        },
        React12.createElement(RemixErrorBoundary, { location: router.state.location }, React12.createElement(
          StaticRouterProvider,
          {
            router,
            context: context.staticHandlerContext,
            hydrate: false
          }
        ))
      ), context.serverHandoffStream ? React12.createElement(React12.Suspense, null, React12.createElement(
        StreamTransfer,
        {
          context,
          identifier: 0,
          reader: context.serverHandoffStream.getReader(),
          textDecoder: new TextDecoder(),
          nonce
        }
      )) : null);
    }
    var React13 = __toESM2(require_react());
    function createRoutesStub(routes, context = {}) {
      return function RoutesTestStub({
        initialEntries,
        initialIndex,
        hydrationData,
        future
      }) {
        let routerRef = React13.useRef();
        let remixContextRef = React13.useRef();
        if (routerRef.current == null) {
          remixContextRef.current = {
            future: {},
            manifest: {
              routes: {},
              entry: { imports: [], module: "" },
              url: "",
              version: ""
            },
            routeModules: {},
            ssr: false,
            isSpaMode: false
          };
          let patched = processRoutes(
            // @ts-expect-error loader/action context types don't match :/
            convertRoutesToDataRoutes(routes, (r) => r),
            context,
            remixContextRef.current.manifest,
            remixContextRef.current.routeModules
          );
          routerRef.current = createMemoryRouter(patched, {
            initialEntries,
            initialIndex,
            hydrationData
          });
        }
        return React13.createElement(FrameworkContext.Provider, { value: remixContextRef.current }, React13.createElement(RouterProvider2, { router: routerRef.current }));
      };
    }
    function processRoutes(routes, context, manifest, routeModules, parentId) {
      return routes.map((route) => {
        if (!route.id) {
          throw new Error(
            "Expected a route.id in @remix-run/testing processRoutes() function"
          );
        }
        let { loader, action } = route;
        let newRoute = {
          id: route.id,
          path: route.path,
          index: route.index,
          Component: route.Component,
          HydrateFallback: route.HydrateFallback,
          ErrorBoundary: route.ErrorBoundary,
          action: action ? (args) => action({ ...args, context }) : void 0,
          loader: loader ? (args) => loader({ ...args, context }) : void 0,
          handle: route.handle,
          shouldRevalidate: route.shouldRevalidate
        };
        let entryRoute = {
          id: route.id,
          path: route.path,
          index: route.index,
          parentId,
          hasAction: route.action != null,
          hasLoader: route.loader != null,
          // When testing routes, you should just be stubbing loader/action, not
          // trying to re-implement the full loader/clientLoader/SSR/hydration flow.
          // That is better tested via E2E tests.
          hasClientAction: false,
          hasClientLoader: false,
          hasErrorBoundary: route.ErrorBoundary != null,
          // any need for these?
          module: "build/stub-path-to-module.js",
          clientActionModule: void 0,
          clientLoaderModule: void 0,
          hydrateFallbackModule: void 0
        };
        manifest.routes[newRoute.id] = entryRoute;
        routeModules[route.id] = {
          default: route.Component || Outlet,
          ErrorBoundary: route.ErrorBoundary || void 0,
          handle: route.handle,
          links: route.links,
          meta: route.meta,
          shouldRevalidate: route.shouldRevalidate
        };
        if (route.children) {
          newRoute.children = processRoutes(
            route.children,
            context,
            manifest,
            routeModules,
            newRoute.id
          );
        }
        return newRoute;
      });
    }
    var import_cookie = require_dist();
    var encoder = new TextEncoder();
    var sign = async (value, secret) => {
      let data2 = encoder.encode(value);
      let key = await createKey2(secret, ["sign"]);
      let signature = await crypto.subtle.sign("HMAC", key, data2);
      let hash = btoa(String.fromCharCode(...new Uint8Array(signature))).replace(
        /=+$/,
        ""
      );
      return value + "." + hash;
    };
    var unsign = async (cookie, secret) => {
      let index = cookie.lastIndexOf(".");
      let value = cookie.slice(0, index);
      let hash = cookie.slice(index + 1);
      let data2 = encoder.encode(value);
      let key = await createKey2(secret, ["verify"]);
      let signature = byteStringToUint8Array(atob(hash));
      let valid = await crypto.subtle.verify("HMAC", key, signature, data2);
      return valid ? value : false;
    };
    var createKey2 = async (secret, usages) => crypto.subtle.importKey(
      "raw",
      encoder.encode(secret),
      { name: "HMAC", hash: "SHA-256" },
      false,
      usages
    );
    function byteStringToUint8Array(byteString) {
      let array = new Uint8Array(byteString.length);
      for (let i = 0; i < byteString.length; i++) {
        array[i] = byteString.charCodeAt(i);
      }
      return array;
    }
    var createCookie = (name, cookieOptions = {}) => {
      let { secrets = [], ...options } = {
        path: "/",
        sameSite: "lax",
        ...cookieOptions
      };
      warnOnceAboutExpiresCookie(name, options.expires);
      return {
        get name() {
          return name;
        },
        get isSigned() {
          return secrets.length > 0;
        },
        get expires() {
          return typeof options.maxAge !== "undefined" ? new Date(Date.now() + options.maxAge * 1e3) : options.expires;
        },
        async parse(cookieHeader, parseOptions) {
          if (!cookieHeader) return null;
          let cookies = (0, import_cookie.parse)(cookieHeader, { ...options, ...parseOptions });
          if (name in cookies) {
            let value = cookies[name];
            if (typeof value === "string" && value !== "") {
              let decoded = await decodeCookieValue(value, secrets);
              return decoded;
            } else {
              return "";
            }
          } else {
            return null;
          }
        },
        async serialize(value, serializeOptions) {
          return (0, import_cookie.serialize)(
            name,
            value === "" ? "" : await encodeCookieValue(value, secrets),
            {
              ...options,
              ...serializeOptions
            }
          );
        }
      };
    };
    var isCookie = (object) => {
      return object != null && typeof object.name === "string" && typeof object.isSigned === "boolean" && typeof object.parse === "function" && typeof object.serialize === "function";
    };
    async function encodeCookieValue(value, secrets) {
      let encoded = encodeData(value);
      if (secrets.length > 0) {
        encoded = await sign(encoded, secrets[0]);
      }
      return encoded;
    }
    async function decodeCookieValue(value, secrets) {
      if (secrets.length > 0) {
        for (let secret of secrets) {
          let unsignedValue = await unsign(value, secret);
          if (unsignedValue !== false) {
            return decodeData(unsignedValue);
          }
        }
        return null;
      }
      return decodeData(value);
    }
    function encodeData(value) {
      return btoa(myUnescape(encodeURIComponent(JSON.stringify(value))));
    }
    function decodeData(value) {
      try {
        return JSON.parse(decodeURIComponent(myEscape(atob(value))));
      } catch (error) {
        return {};
      }
    }
    function myEscape(value) {
      let str = value.toString();
      let result = "";
      let index = 0;
      let chr, code;
      while (index < str.length) {
        chr = str.charAt(index++);
        if (/[\w*+\-./@]/.exec(chr)) {
          result += chr;
        } else {
          code = chr.charCodeAt(0);
          if (code < 256) {
            result += "%" + hex(code, 2);
          } else {
            result += "%u" + hex(code, 4).toUpperCase();
          }
        }
      }
      return result;
    }
    function hex(code, length) {
      let result = code.toString(16);
      while (result.length < length) result = "0" + result;
      return result;
    }
    function myUnescape(value) {
      let str = value.toString();
      let result = "";
      let index = 0;
      let chr, part;
      while (index < str.length) {
        chr = str.charAt(index++);
        if (chr === "%") {
          if (str.charAt(index) === "u") {
            part = str.slice(index + 1, index + 5);
            if (/^[\da-f]{4}$/i.exec(part)) {
              result += String.fromCharCode(parseInt(part, 16));
              index += 5;
              continue;
            }
          } else {
            part = str.slice(index, index + 2);
            if (/^[\da-f]{2}$/i.exec(part)) {
              result += String.fromCharCode(parseInt(part, 16));
              index += 2;
              continue;
            }
          }
        }
        result += chr;
      }
      return result;
    }
    function warnOnceAboutExpiresCookie(name, expires) {
      warnOnce(
        !expires,
        `The "${name}" cookie has an "expires" property set. This will cause the expires value to not be updated when the session is committed. Instead, you should set the expires value when serializing the cookie. You can use \`commitSession(session, { expires })\` if using a session storage object, or \`cookie.serialize("value", { expires })\` if you're using the cookie directly.`
      );
    }
    function createEntryRouteModules(manifest) {
      return Object.keys(manifest).reduce((memo2, routeId) => {
        let route = manifest[routeId];
        if (route) {
          memo2[routeId] = route.module;
        }
        return memo2;
      }, {});
    }
    var ServerMode = ((ServerMode2) => {
      ServerMode2["Development"] = "development";
      ServerMode2["Production"] = "production";
      ServerMode2["Test"] = "test";
      return ServerMode2;
    })(ServerMode || {});
    function isServerMode(value) {
      return value === "development" || value === "production" || value === "test";
    }
    function sanitizeError(error, serverMode) {
      if (error instanceof Error && serverMode !== "development") {
        let sanitized = new Error("Unexpected Server Error");
        sanitized.stack = void 0;
        return sanitized;
      }
      return error;
    }
    function sanitizeErrors(errors, serverMode) {
      return Object.entries(errors).reduce((acc, [routeId, error]) => {
        return Object.assign(acc, { [routeId]: sanitizeError(error, serverMode) });
      }, {});
    }
    function serializeError(error, serverMode) {
      let sanitized = sanitizeError(error, serverMode);
      return {
        message: sanitized.message,
        stack: sanitized.stack
      };
    }
    function serializeErrors2(errors, serverMode) {
      if (!errors) return null;
      let entries = Object.entries(errors);
      let serialized = {};
      for (let [key, val] of entries) {
        if (isRouteErrorResponse(val)) {
          serialized[key] = { ...val, __type: "RouteErrorResponse" };
        } else if (val instanceof Error) {
          let sanitized = sanitizeError(val, serverMode);
          serialized[key] = {
            message: sanitized.message,
            stack: sanitized.stack,
            __type: "Error",
            // If this is a subclass (i.e., ReferenceError), send up the type so we
            // can re-create the same type during hydration.  This will only apply
            // in dev mode since all production errors are sanitized to normal
            // Error instances
            ...sanitized.name !== "Error" ? {
              __subType: sanitized.name
            } : {}
          };
        } else {
          serialized[key] = val;
        }
      }
      return serialized;
    }
    function matchServerRoutes(routes, pathname, basename) {
      let matches = matchRoutes(
        routes,
        pathname,
        basename
      );
      if (!matches) return null;
      return matches.map((match) => ({
        params: match.params,
        pathname: match.pathname,
        route: match.route
      }));
    }
    async function callRouteHandler(handler, args) {
      let result = await handler({
        request: stripRoutesParam(stripIndexParam2(args.request)),
        params: args.params,
        context: args.context
      });
      if (isDataWithResponseInit(result) && result.init && result.init.status && isRedirectStatusCode(result.init.status)) {
        throw new Response(null, result.init);
      }
      return result;
    }
    function stripIndexParam2(request) {
      let url = new URL(request.url);
      let indexValues = url.searchParams.getAll("index");
      url.searchParams.delete("index");
      let indexValuesToKeep = [];
      for (let indexValue of indexValues) {
        if (indexValue) {
          indexValuesToKeep.push(indexValue);
        }
      }
      for (let toKeep of indexValuesToKeep) {
        url.searchParams.append("index", toKeep);
      }
      let init = {
        method: request.method,
        body: request.body,
        headers: request.headers,
        signal: request.signal
      };
      if (init.body) {
        init.duplex = "half";
      }
      return new Request(url.href, init);
    }
    function stripRoutesParam(request) {
      let url = new URL(request.url);
      url.searchParams.delete("_routes");
      let init = {
        method: request.method,
        body: request.body,
        headers: request.headers,
        signal: request.signal
      };
      if (init.body) {
        init.duplex = "half";
      }
      return new Request(url.href, init);
    }
    function invariant3(value, message) {
      if (value === false || value === null || typeof value === "undefined") {
        console.error(
          "The following error is a bug in React Router; please open an issue! https://github.com/remix-run/react-router/issues/new/choose"
        );
        throw new Error(message);
      }
    }
    function groupRoutesByParentId2(manifest) {
      let routes = {};
      Object.values(manifest).forEach((route) => {
        if (route) {
          let parentId = route.parentId || "";
          if (!routes[parentId]) {
            routes[parentId] = [];
          }
          routes[parentId].push(route);
        }
      });
      return routes;
    }
    function createRoutes(manifest, parentId = "", routesByParentId = groupRoutesByParentId2(manifest)) {
      return (routesByParentId[parentId] || []).map((route) => ({
        ...route,
        children: createRoutes(manifest, route.id, routesByParentId)
      }));
    }
    function createStaticHandlerDataRoutes(manifest, future, parentId = "", routesByParentId = groupRoutesByParentId2(manifest)) {
      return (routesByParentId[parentId] || []).map((route) => {
        let commonRoute = {
          // Always include root due to default boundaries
          hasErrorBoundary: route.id === "root" || route.module.ErrorBoundary != null,
          id: route.id,
          path: route.path,
          // Need to use RR's version in the param typed here to permit the optional
          // context even though we know it'll always be provided in remix
          loader: route.module.loader ? async (args) => {
            if (args.request.headers.has("X-React-Router-Prerender-Data")) {
              const preRenderedData = args.request.headers.get(
                "X-React-Router-Prerender-Data"
              );
              let encoded = preRenderedData ? decodeURI(preRenderedData) : preRenderedData;
              invariant3(encoded, "Missing prerendered data for route");
              let uint8array = new TextEncoder().encode(encoded);
              let stream = new ReadableStream({
                start(controller) {
                  controller.enqueue(uint8array);
                  controller.close();
                }
              });
              let decoded = await decodeViaTurboStream(stream, global);
              let data2 = decoded.value;
              invariant3(
                data2 && route.id in data2,
                "Unable to decode prerendered data"
              );
              let result = data2[route.id];
              invariant3("data" in result, "Unable to process prerendered data");
              return result.data;
            }
            let val = await callRouteHandler(route.module.loader, args);
            return val;
          } : void 0,
          action: route.module.action ? (args) => callRouteHandler(route.module.action, args) : void 0,
          handle: route.module.handle
        };
        return route.index ? {
          index: true,
          ...commonRoute
        } : {
          caseSensitive: route.caseSensitive,
          children: createStaticHandlerDataRoutes(
            manifest,
            future,
            route.id,
            routesByParentId
          ),
          ...commonRoute
        };
      });
    }
    var ESCAPE_LOOKUP3 = {
      "&": "\\u0026",
      ">": "\\u003e",
      "<": "\\u003c",
      "\u2028": "\\u2028",
      "\u2029": "\\u2029"
    };
    var ESCAPE_REGEX3 = /[&><\u2028\u2029]/g;
    function escapeHtml2(html) {
      return html.replace(ESCAPE_REGEX3, (match) => ESCAPE_LOOKUP3[match]);
    }
    function createServerHandoffString(serverHandoff) {
      return escapeHtml2(JSON.stringify(serverHandoff));
    }
    var globalDevServerHooksKey = "__reactRouterDevServerHooks";
    function setDevServerHooks(devServerHooks) {
      globalThis[globalDevServerHooksKey] = devServerHooks;
    }
    function getDevServerHooks() {
      return globalThis[globalDevServerHooksKey];
    }
    var import_turbo_stream2 = require_turbo_stream();
    var import_set_cookie_parser = require_set_cookie();
    function getDocumentHeaders(build, context) {
      let boundaryIdx = context.errors ? context.matches.findIndex((m) => context.errors[m.route.id]) : -1;
      let matches = boundaryIdx >= 0 ? context.matches.slice(0, boundaryIdx + 1) : context.matches;
      let errorHeaders;
      if (boundaryIdx >= 0) {
        let { actionHeaders, actionData, loaderHeaders, loaderData } = context;
        context.matches.slice(boundaryIdx).some((match) => {
          let id = match.route.id;
          if (actionHeaders[id] && (!actionData || !actionData.hasOwnProperty(id))) {
            errorHeaders = actionHeaders[id];
          } else if (loaderHeaders[id] && !loaderData.hasOwnProperty(id)) {
            errorHeaders = loaderHeaders[id];
          }
          return errorHeaders != null;
        });
      }
      return matches.reduce((parentHeaders, match, idx) => {
        let { id } = match.route;
        let route = build.routes[id];
        invariant3(route, `Route with id "${id}" not found in build`);
        let routeModule = route.module;
        let loaderHeaders = context.loaderHeaders[id] || new Headers();
        let actionHeaders = context.actionHeaders[id] || new Headers();
        let includeErrorHeaders = errorHeaders != null && idx === matches.length - 1;
        let includeErrorCookies = includeErrorHeaders && errorHeaders !== loaderHeaders && errorHeaders !== actionHeaders;
        if (routeModule.headers == null) {
          let headers2 = new Headers(parentHeaders);
          if (includeErrorCookies) {
            prependCookies(errorHeaders, headers2);
          }
          prependCookies(actionHeaders, headers2);
          prependCookies(loaderHeaders, headers2);
          return headers2;
        }
        let headers = new Headers(
          routeModule.headers ? typeof routeModule.headers === "function" ? routeModule.headers({
            loaderHeaders,
            parentHeaders,
            actionHeaders,
            errorHeaders: includeErrorHeaders ? errorHeaders : void 0
          }) : routeModule.headers : void 0
        );
        if (includeErrorCookies) {
          prependCookies(errorHeaders, headers);
        }
        prependCookies(actionHeaders, headers);
        prependCookies(loaderHeaders, headers);
        prependCookies(parentHeaders, headers);
        return headers;
      }, new Headers());
    }
    function prependCookies(parentHeaders, childHeaders) {
      let parentSetCookieString = parentHeaders.get("Set-Cookie");
      if (parentSetCookieString) {
        let cookies = (0, import_set_cookie_parser.splitCookiesString)(parentSetCookieString);
        let childCookies = new Set(childHeaders.getSetCookie());
        cookies.forEach((cookie) => {
          if (!childCookies.has(cookie)) {
            childHeaders.append("Set-Cookie", cookie);
          }
        });
      }
    }
    var SINGLE_FETCH_REDIRECT_STATUS = 202;
    function getSingleFetchDataStrategy2({
      isActionDataRequest,
      loadRouteIds
    } = {}) {
      return async ({ request, matches }) => {
        if (isActionDataRequest && request.method === "GET") {
          return {};
        }
        let matchesToLoad = loadRouteIds ? matches.filter((m) => loadRouteIds.includes(m.route.id)) : matches;
        let results = await Promise.all(
          matchesToLoad.map((match) => match.resolve())
        );
        return results.reduce(
          (acc, result, i) => Object.assign(acc, { [matchesToLoad[i].route.id]: result }),
          {}
        );
      };
    }
    async function singleFetchAction(build, serverMode, staticHandler, request, handlerUrl, loadContext, handleError) {
      try {
        let handlerRequest = new Request(handlerUrl, {
          method: request.method,
          body: request.body,
          headers: request.headers,
          signal: request.signal,
          ...request.body ? { duplex: "half" } : void 0
        });
        let result = await staticHandler.query(handlerRequest, {
          requestContext: loadContext,
          skipLoaderErrorBubbling: true,
          dataStrategy: getSingleFetchDataStrategy2({
            isActionDataRequest: true
          })
        });
        if (isResponse(result)) {
          return {
            result: getSingleFetchRedirect(
              result.status,
              result.headers,
              build.basename
            ),
            headers: result.headers,
            status: SINGLE_FETCH_REDIRECT_STATUS
          };
        }
        let context = result;
        let headers = getDocumentHeaders(build, context);
        if (isRedirectStatusCode(context.statusCode) && headers.has("Location")) {
          return {
            result: getSingleFetchRedirect(
              context.statusCode,
              headers,
              build.basename
            ),
            headers,
            status: SINGLE_FETCH_REDIRECT_STATUS
          };
        }
        if (context.errors) {
          Object.values(context.errors).forEach((err) => {
            if (!isRouteErrorResponse(err) || err.error) {
              handleError(err);
            }
          });
          context.errors = sanitizeErrors(context.errors, serverMode);
        }
        let singleFetchResult;
        if (context.errors) {
          singleFetchResult = { error: Object.values(context.errors)[0] };
        } else {
          singleFetchResult = { data: Object.values(context.actionData || {})[0] };
        }
        return {
          result: singleFetchResult,
          headers,
          status: context.statusCode
        };
      } catch (error) {
        handleError(error);
        return {
          result: { error },
          headers: new Headers(),
          status: 500
        };
      }
    }
    async function singleFetchLoaders(build, serverMode, staticHandler, request, handlerUrl, loadContext, handleError) {
      var _a;
      try {
        let handlerRequest = new Request(handlerUrl, {
          headers: request.headers,
          signal: request.signal
        });
        let loadRouteIds = ((_a = new URL(request.url).searchParams.get("_routes")) == null ? void 0 : _a.split(",")) || void 0;
        let result = await staticHandler.query(handlerRequest, {
          requestContext: loadContext,
          skipLoaderErrorBubbling: true,
          dataStrategy: getSingleFetchDataStrategy2({
            loadRouteIds
          })
        });
        if (isResponse(result)) {
          return {
            result: {
              [SingleFetchRedirectSymbol]: getSingleFetchRedirect(
                result.status,
                result.headers,
                build.basename
              )
            },
            headers: result.headers,
            status: SINGLE_FETCH_REDIRECT_STATUS
          };
        }
        let context = result;
        let headers = getDocumentHeaders(build, context);
        if (isRedirectStatusCode(context.statusCode) && headers.has("Location")) {
          return {
            result: {
              [SingleFetchRedirectSymbol]: getSingleFetchRedirect(
                context.statusCode,
                headers,
                build.basename
              )
            },
            headers,
            status: SINGLE_FETCH_REDIRECT_STATUS
          };
        }
        if (context.errors) {
          Object.values(context.errors).forEach((err) => {
            if (!isRouteErrorResponse(err) || err.error) {
              handleError(err);
            }
          });
          context.errors = sanitizeErrors(context.errors, serverMode);
        }
        let results = {};
        let loadedMatches = loadRouteIds ? context.matches.filter(
          (m) => m.route.loader && loadRouteIds.includes(m.route.id)
        ) : context.matches;
        loadedMatches.forEach((m) => {
          let { id } = m.route;
          if (context.errors && context.errors.hasOwnProperty(id)) {
            results[id] = { error: context.errors[id] };
          } else if (context.loaderData.hasOwnProperty(id)) {
            results[id] = { data: context.loaderData[id] };
          }
        });
        return {
          result: results,
          headers,
          status: context.statusCode
        };
      } catch (error) {
        handleError(error);
        return {
          result: { root: { error } },
          headers: new Headers(),
          status: 500
        };
      }
    }
    function getSingleFetchRedirect(status, headers, basename) {
      let redirect2 = headers.get("Location");
      if (basename) {
        redirect2 = stripBasename(redirect2, basename) || redirect2;
      }
      return {
        redirect: redirect2,
        status,
        revalidate: (
          // Technically X-Remix-Revalidate isn't needed here - that was an implementation
          // detail of ?_data requests as our way to tell the front end to revalidate when
          // we didn't have a response body to include that information in.
          // With single fetch, we tell the front end via this revalidate boolean field.
          // However, we're respecting it for now because it may be something folks have
          // used in their own responses
          // TODO(v3): Consider removing or making this official public API
          headers.has("X-Remix-Revalidate") || headers.has("Set-Cookie")
        ),
        reload: headers.has("X-Remix-Reload-Document"),
        replace: headers.has("X-Remix-Replace")
      };
    }
    function encodeViaTurboStream(data2, requestSignal, streamTimeout, serverMode) {
      let controller = new AbortController();
      let timeoutId = setTimeout(
        () => controller.abort(new Error("Server Timeout")),
        typeof streamTimeout === "number" ? streamTimeout : 4950
      );
      requestSignal.addEventListener("abort", () => clearTimeout(timeoutId));
      return (0, import_turbo_stream2.encode)(data2, {
        signal: controller.signal,
        plugins: [
          (value) => {
            if (value instanceof Error) {
              let { name, message, stack } = serverMode === "production" ? sanitizeError(value, serverMode) : value;
              return ["SanitizedError", name, message, stack];
            }
            if (value instanceof ErrorResponseImpl) {
              let { data: data3, status, statusText } = value;
              return ["ErrorResponse", data3, status, statusText];
            }
            if (value && typeof value === "object" && SingleFetchRedirectSymbol in value) {
              return ["SingleFetchRedirect", value[SingleFetchRedirectSymbol]];
            }
          }
        ],
        postPlugins: [
          (value) => {
            if (!value) return;
            if (typeof value !== "object") return;
            return [
              "SingleFetchClassInstance",
              Object.fromEntries(Object.entries(value))
            ];
          },
          () => ["SingleFetchFallback"]
        ]
      });
    }
    var NO_BODY_STATUS_CODES = /* @__PURE__ */ new Set([100, 101, 204, 205, 304]);
    function derive(build, mode) {
      let routes = createRoutes(build.routes);
      let dataRoutes = createStaticHandlerDataRoutes(build.routes, build.future);
      let serverMode = isServerMode(mode) ? mode : "production";
      let staticHandler = createStaticHandler(dataRoutes, {
        basename: build.basename
      });
      let errorHandler = build.entry.module.handleError || ((error, { request }) => {
        if (serverMode !== "test" && !request.signal.aborted) {
          console.error(
            // @ts-expect-error This is "private" from users but intended for internal use
            isRouteErrorResponse(error) && error.error ? error.error : error
          );
        }
      });
      return {
        routes,
        dataRoutes,
        serverMode,
        staticHandler,
        errorHandler
      };
    }
    var createRequestHandler = (build, mode) => {
      let _build;
      let routes;
      let serverMode;
      let staticHandler;
      let errorHandler;
      return async function requestHandler(request, loadContext = {}) {
        var _a, _b;
        _build = typeof build === "function" ? await build() : build;
        if (typeof build === "function") {
          let derived = derive(_build, mode);
          routes = derived.routes;
          serverMode = derived.serverMode;
          staticHandler = derived.staticHandler;
          errorHandler = derived.errorHandler;
        } else if (!routes || !serverMode || !staticHandler || !errorHandler) {
          let derived = derive(_build, mode);
          routes = derived.routes;
          serverMode = derived.serverMode;
          staticHandler = derived.staticHandler;
          errorHandler = derived.errorHandler;
        }
        let url = new URL(request.url);
        let normalizedPath = url.pathname.replace(/\.data$/, "").replace(/^\/_root$/, "/");
        if (normalizedPath !== "/" && normalizedPath.endsWith("/")) {
          normalizedPath = normalizedPath.slice(0, -1);
        }
        let params = {};
        let handleError = (error) => {
          var _a2, _b2;
          if (mode === "development") {
            (_b2 = (_a2 = getDevServerHooks()) == null ? void 0 : _a2.processRequestError) == null ? void 0 : _b2.call(_a2, error);
          }
          errorHandler(error, {
            context: loadContext,
            params,
            request
          });
        };
        if (!_build.ssr) {
          if (_build.prerender.length === 0) {
            request.headers.set("X-React-Router-SPA-Mode", "yes");
          } else if (!_build.prerender.includes(normalizedPath) && !_build.prerender.includes(normalizedPath + "/")) {
            if (url.pathname.endsWith(".data")) {
              errorHandler(
                new ErrorResponseImpl(
                  404,
                  "Not Found",
                  `Refusing to SSR the path \`${normalizedPath}\` because \`ssr:false\` is set and the path is not included in the \`prerender\` config, so in production the path will be a 404.`
                ),
                {
                  context: loadContext,
                  params,
                  request
                }
              );
              return new Response("Not Found", {
                status: 404,
                statusText: "Not Found"
              });
            } else {
              request.headers.set("X-React-Router-SPA-Mode", "yes");
            }
          }
        }
        let manifestUrl = `${_build.basename ?? "/"}/__manifest`.replace(
          /\/+/g,
          "/"
        );
        if (url.pathname === manifestUrl) {
          try {
            let res = await handleManifestRequest(_build, routes, url);
            return res;
          } catch (e) {
            handleError(e);
            return new Response("Unknown Server Error", { status: 500 });
          }
        }
        let matches = matchServerRoutes(routes, url.pathname, _build.basename);
        if (matches && matches.length > 0) {
          Object.assign(params, matches[0].params);
        }
        let response;
        if (url.pathname.endsWith(".data")) {
          let handlerUrl = new URL(request.url);
          handlerUrl.pathname = normalizedPath;
          let singleFetchMatches = matchServerRoutes(
            routes,
            handlerUrl.pathname,
            _build.basename
          );
          response = await handleSingleFetchRequest(
            serverMode,
            _build,
            staticHandler,
            request,
            handlerUrl,
            loadContext,
            handleError
          );
          if (_build.entry.module.handleDataRequest) {
            response = await _build.entry.module.handleDataRequest(response, {
              context: loadContext,
              params: singleFetchMatches ? singleFetchMatches[0].params : {},
              request
            });
            if (isRedirectResponse(response)) {
              let result = getSingleFetchRedirect(
                response.status,
                response.headers,
                _build.basename
              );
              if (request.method === "GET") {
                result = {
                  [SingleFetchRedirectSymbol]: result
                };
              }
              let headers = new Headers(response.headers);
              headers.set("Content-Type", "text/x-script");
              return new Response(
                encodeViaTurboStream(
                  result,
                  request.signal,
                  _build.entry.module.streamTimeout,
                  serverMode
                ),
                {
                  status: SINGLE_FETCH_REDIRECT_STATUS,
                  headers
                }
              );
            }
          }
        } else if (matches && matches[matches.length - 1].route.module.default == null && matches[matches.length - 1].route.module.ErrorBoundary == null) {
          response = await handleResourceRequest(
            serverMode,
            staticHandler,
            matches.slice(-1)[0].route.id,
            request,
            loadContext,
            handleError
          );
        } else {
          let criticalCss = mode === "development" ? await ((_b = (_a = getDevServerHooks()) == null ? void 0 : _a.getCriticalCss) == null ? void 0 : _b.call(_a, _build, url.pathname)) : void 0;
          response = await handleDocumentRequest(
            serverMode,
            _build,
            staticHandler,
            request,
            loadContext,
            handleError,
            criticalCss
          );
        }
        if (request.method === "HEAD") {
          return new Response(null, {
            headers: response.headers,
            status: response.status,
            statusText: response.statusText
          });
        }
        return response;
      };
    };
    async function handleManifestRequest(build, routes, url) {
      let patches = {};
      if (url.searchParams.has("p")) {
        for (let path of url.searchParams.getAll("p")) {
          let matches = matchServerRoutes(routes, path, build.basename);
          if (matches) {
            for (let match of matches) {
              let routeId = match.route.id;
              let route = build.assets.routes[routeId];
              if (route) {
                patches[routeId] = route;
              }
            }
          }
        }
        return Response.json(patches, {
          headers: {
            "Cache-Control": "public, max-age=31536000, immutable"
          }
        });
      }
      return new Response("Invalid Request", { status: 400 });
    }
    async function handleSingleFetchRequest(serverMode, build, staticHandler, request, handlerUrl, loadContext, handleError) {
      let { result, headers, status } = request.method !== "GET" ? await singleFetchAction(
        build,
        serverMode,
        staticHandler,
        request,
        handlerUrl,
        loadContext,
        handleError
      ) : await singleFetchLoaders(
        build,
        serverMode,
        staticHandler,
        request,
        handlerUrl,
        loadContext,
        handleError
      );
      let resultHeaders = new Headers(headers);
      resultHeaders.set("X-Remix-Response", "yes");
      if (NO_BODY_STATUS_CODES.has(status)) {
        return new Response(null, { status, headers: resultHeaders });
      }
      resultHeaders.set("Content-Type", "text/x-script");
      return new Response(
        encodeViaTurboStream(
          result,
          request.signal,
          build.entry.module.streamTimeout,
          serverMode
        ),
        {
          status: status || 200,
          headers: resultHeaders
        }
      );
    }
    async function handleDocumentRequest(serverMode, build, staticHandler, request, loadContext, handleError, criticalCss) {
      let isSpaMode = request.headers.has("X-React-Router-SPA-Mode");
      let context;
      try {
        context = await staticHandler.query(request, {
          requestContext: loadContext
        });
      } catch (error) {
        handleError(error);
        return new Response(null, { status: 500 });
      }
      if (isResponse(context)) {
        return context;
      }
      let headers = getDocumentHeaders(build, context);
      if (NO_BODY_STATUS_CODES.has(context.statusCode)) {
        return new Response(null, { status: context.statusCode, headers });
      }
      if (context.errors) {
        Object.values(context.errors).forEach((err) => {
          if (!isRouteErrorResponse(err) || err.error) {
            handleError(err);
          }
        });
        context.errors = sanitizeErrors(context.errors, serverMode);
      }
      let state = {
        loaderData: context.loaderData,
        actionData: context.actionData,
        errors: serializeErrors2(context.errors, serverMode)
      };
      let entryContext = {
        manifest: build.assets,
        routeModules: createEntryRouteModules(build.routes),
        staticHandlerContext: context,
        criticalCss,
        serverHandoffString: createServerHandoffString({
          basename: build.basename,
          criticalCss,
          future: build.future,
          ssr: build.ssr,
          isSpaMode
        }),
        serverHandoffStream: encodeViaTurboStream(
          state,
          request.signal,
          build.entry.module.streamTimeout,
          serverMode
        ),
        renderMeta: {},
        future: build.future,
        ssr: build.ssr,
        isSpaMode,
        serializeError: (err) => serializeError(err, serverMode)
      };
      let handleDocumentRequestFunction = build.entry.module.default;
      try {
        return await handleDocumentRequestFunction(
          request,
          context.statusCode,
          headers,
          entryContext,
          loadContext
        );
      } catch (error) {
        handleError(error);
        let errorForSecondRender = error;
        if (isResponse(error)) {
          try {
            let data2 = await unwrapResponse(error);
            errorForSecondRender = new ErrorResponseImpl(
              error.status,
              error.statusText,
              data2
            );
          } catch (e) {
          }
        }
        context = getStaticContextFromError(
          staticHandler.dataRoutes,
          context,
          errorForSecondRender
        );
        if (context.errors) {
          context.errors = sanitizeErrors(context.errors, serverMode);
        }
        let state2 = {
          loaderData: context.loaderData,
          actionData: context.actionData,
          errors: serializeErrors2(context.errors, serverMode)
        };
        entryContext = {
          ...entryContext,
          staticHandlerContext: context,
          serverHandoffString: createServerHandoffString({
            basename: build.basename,
            future: build.future,
            ssr: build.ssr,
            isSpaMode
          }),
          serverHandoffStream: encodeViaTurboStream(
            state2,
            request.signal,
            build.entry.module.streamTimeout,
            serverMode
          ),
          renderMeta: {}
        };
        try {
          return await handleDocumentRequestFunction(
            request,
            context.statusCode,
            headers,
            entryContext,
            loadContext
          );
        } catch (error2) {
          handleError(error2);
          return returnLastResortErrorResponse(error2, serverMode);
        }
      }
    }
    async function handleResourceRequest(serverMode, staticHandler, routeId, request, loadContext, handleError) {
      try {
        let response = await staticHandler.queryRoute(request, {
          routeId,
          requestContext: loadContext
        });
        if (isResponse(response)) {
          return response;
        }
        if (typeof response === "string") {
          return new Response(response);
        }
        return Response.json(response);
      } catch (error) {
        if (isResponse(error)) {
          error.headers.set("X-Remix-Catch", "yes");
          return error;
        }
        if (isRouteErrorResponse(error)) {
          if (error) {
            handleError(error);
          }
          return errorResponseToJson(error, serverMode);
        }
        handleError(error);
        return returnLastResortErrorResponse(error, serverMode);
      }
    }
    function errorResponseToJson(errorResponse, serverMode) {
      return Response.json(
        serializeError(
          // @ts-expect-error This is "private" from users but intended for internal use
          errorResponse.error || new Error("Unexpected Server Error"),
          serverMode
        ),
        {
          status: errorResponse.status,
          statusText: errorResponse.statusText,
          headers: {
            "X-Remix-Error": "yes"
          }
        }
      );
    }
    function returnLastResortErrorResponse(error, serverMode) {
      let message = "Unexpected Server Error";
      if (serverMode !== "production") {
        message += `

${String(error)}`;
      }
      return new Response(message, {
        status: 500,
        headers: {
          "Content-Type": "text/plain"
        }
      });
    }
    function unwrapResponse(response) {
      let contentType = response.headers.get("Content-Type");
      return contentType && /\bapplication\/json\b/.test(contentType) ? response.body == null ? null : response.json() : response.text();
    }
    function flash(name) {
      return `__flash_${name}__`;
    }
    var createSession = (initialData = {}, id = "") => {
      let map = new Map(Object.entries(initialData));
      return {
        get id() {
          return id;
        },
        get data() {
          return Object.fromEntries(map);
        },
        has(name) {
          return map.has(name) || map.has(flash(name));
        },
        get(name) {
          if (map.has(name)) return map.get(name);
          let flashName = flash(name);
          if (map.has(flashName)) {
            let value = map.get(flashName);
            map.delete(flashName);
            return value;
          }
          return void 0;
        },
        set(name, value) {
          map.set(name, value);
        },
        flash(name, value) {
          map.set(flash(name), value);
        },
        unset(name) {
          map.delete(name);
        }
      };
    };
    var isSession = (object) => {
      return object != null && typeof object.id === "string" && typeof object.data !== "undefined" && typeof object.has === "function" && typeof object.get === "function" && typeof object.set === "function" && typeof object.flash === "function" && typeof object.unset === "function";
    };
    function createSessionStorage({
      cookie: cookieArg,
      createData,
      readData,
      updateData,
      deleteData
    }) {
      let cookie = isCookie(cookieArg) ? cookieArg : createCookie((cookieArg == null ? void 0 : cookieArg.name) || "__session", cookieArg);
      warnOnceAboutSigningSessionCookie(cookie);
      return {
        async getSession(cookieHeader, options) {
          let id = cookieHeader && await cookie.parse(cookieHeader, options);
          let data2 = id && await readData(id);
          return createSession(data2 || {}, id || "");
        },
        async commitSession(session, options) {
          let { id, data: data2 } = session;
          let expires = (options == null ? void 0 : options.maxAge) != null ? new Date(Date.now() + options.maxAge * 1e3) : (options == null ? void 0 : options.expires) != null ? options.expires : cookie.expires;
          if (id) {
            await updateData(id, data2, expires);
          } else {
            id = await createData(data2, expires);
          }
          return cookie.serialize(id, options);
        },
        async destroySession(session, options) {
          await deleteData(session.id);
          return cookie.serialize("", {
            ...options,
            maxAge: void 0,
            expires: /* @__PURE__ */ new Date(0)
          });
        }
      };
    }
    function warnOnceAboutSigningSessionCookie(cookie) {
      warnOnce(
        cookie.isSigned,
        `The "${cookie.name}" cookie is not signed, but session cookies should be signed to prevent tampering on the client before they are sent back to the server. See https://remix.run/utils/cookies#signing-cookies for more information.`
      );
    }
    function createCookieSessionStorage({ cookie: cookieArg } = {}) {
      let cookie = isCookie(cookieArg) ? cookieArg : createCookie((cookieArg == null ? void 0 : cookieArg.name) || "__session", cookieArg);
      warnOnceAboutSigningSessionCookie(cookie);
      return {
        async getSession(cookieHeader, options) {
          return createSession(
            cookieHeader && await cookie.parse(cookieHeader, options) || {}
          );
        },
        async commitSession(session, options) {
          let serializedCookie = await cookie.serialize(session.data, options);
          if (serializedCookie.length > 4096) {
            throw new Error(
              "Cookie length will exceed browser maximum. Length: " + serializedCookie.length
            );
          }
          return serializedCookie;
        },
        async destroySession(_session, options) {
          return cookie.serialize("", {
            ...options,
            maxAge: void 0,
            expires: /* @__PURE__ */ new Date(0)
          });
        }
      };
    }
    function createMemorySessionStorage({ cookie } = {}) {
      let map = /* @__PURE__ */ new Map();
      return createSessionStorage({
        cookie,
        async createData(data2, expires) {
          let id = Math.random().toString(36).substring(2, 10);
          map.set(id, { data: data2, expires });
          return id;
        },
        async readData(id) {
          if (map.has(id)) {
            let { data: data2, expires } = map.get(id);
            if (!expires || expires > /* @__PURE__ */ new Date()) {
              return data2;
            }
            if (expires) map.delete(id);
          }
          return null;
        },
        async updateData(id, data2, expires) {
          map.set(id, { data: data2, expires });
        },
        async deleteData(id) {
          map.delete(id);
        }
      });
    }
    function href(path, ...args) {
      let params = args[0];
      return path.split("/").map((segment) => {
        const match = segment.match(/^:([\w-]+)(\?)?/);
        if (!match) return segment;
        const param = match[1];
        const value = params ? params[param] : void 0;
        const isRequired = match[2] === void 0;
        if (isRequired && value === void 0) {
          throw Error(
            `Path '${path}' requires param '${param}' but it was not provided`
          );
        }
        return value;
      }).filter((segment) => segment !== void 0).join("/");
    }
    function deserializeErrors2(errors) {
      if (!errors) return null;
      let entries = Object.entries(errors);
      let serialized = {};
      for (let [key, val] of entries) {
        if (val && val.__type === "RouteErrorResponse") {
          serialized[key] = new ErrorResponseImpl(
            val.status,
            val.statusText,
            val.data,
            val.internal === true
          );
        } else if (val && val.__type === "Error") {
          if (val.__subType) {
            let ErrorConstructor = window[val.__subType];
            if (typeof ErrorConstructor === "function") {
              try {
                let error = new ErrorConstructor(val.message);
                error.stack = val.stack;
                serialized[key] = error;
              } catch (e) {
              }
            }
          }
          if (serialized[key] == null) {
            let error = new Error(val.message);
            error.stack = val.stack;
            serialized[key] = error;
          }
        } else {
          serialized[key] = val;
        }
      }
      return serialized;
    }
  }
});

// node_modules/react-router-dom/dist/index.js
var require_dist2 = __commonJS({
  "node_modules/react-router-dom/dist/index.js"(exports, module) {
    "use strict";
    var __defProp = Object.defineProperty;
    var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
    var __getOwnPropNames = Object.getOwnPropertyNames;
    var __hasOwnProp = Object.prototype.hasOwnProperty;
    var __export2 = (target, all) => {
      for (var name in all)
        __defProp(target, name, { get: all[name], enumerable: true });
    };
    var __copyProps = (to, from, except, desc) => {
      if (from && typeof from === "object" || typeof from === "function") {
        for (let key of __getOwnPropNames(from))
          if (!__hasOwnProp.call(to, key) && key !== except)
            __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
      }
      return to;
    };
    var __reExport2 = (target, mod, secondTarget) => (__copyProps(target, mod, "default"), secondTarget && __copyProps(secondTarget, mod, "default"));
    var __toCommonJS2 = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
    var react_router_dom_exports = {};
    __export2(react_router_dom_exports, {
      HydratedRouter: () => import_dom.HydratedRouter,
      RouterProvider: () => import_dom.RouterProvider
    });
    module.exports = __toCommonJS2(react_router_dom_exports);
    var import_dom = require_dom_export();
    __reExport2(react_router_dom_exports, require_development(), module.exports);
  }
});

// node_modules/govuk-react-jsx/utils/Link.js
var require_Link = __commonJS({
  "node_modules/govuk-react-jsx/utils/Link.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Link = void 0;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _reactRouterDom = require_dist2();
    var _excluded = ["children", "to", "href", "forwardedRef"];
    function Link(props) {
      var children = props.children, to = props.to, href = props.href, forwardedRef = props.forwardedRef, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      if (to) {
        return _react["default"].createElement(_reactRouterDom.Link, (0, _extends2["default"])({
          innerRef: forwardedRef,
          to
        }, attributes), children);
      }
      return _react["default"].createElement("a", (0, _extends2["default"])({
        ref: forwardedRef,
        href: href || "#"
      }, attributes), children);
    }
    Link.defaultProps = {
      forwardedRef: null
    };
    function forwardRef(props, ref) {
      return _react["default"].createElement(Link, (0, _extends2["default"])({}, props, {
        forwardedRef: ref
      }));
    }
    forwardRef.displayName = "LinkWithRef";
    var LinkWithRef = _react["default"].forwardRef(forwardRef);
    exports.Link = LinkWithRef;
  }
});

// node_modules/govuk-react-jsx/govuk/components/back-link/index.js
var require_back_link = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/back-link/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.BackLink = BackLink;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _Link = require_Link();
    var _excluded = ["children", "href", "to", "className"];
    function BackLink(props) {
      var children = props.children, href = props.href, to = props.to, className = props.className, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var contents = children;
      return _react["default"].createElement(_Link.Link, (0, _extends2["default"])({}, attributes, {
        className: "govuk-back-link ".concat(className || ""),
        href,
        to
      }), contents);
    }
    BackLink.defaultProps = {
      href: "/",
      children: "Back"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/breadcrumbs/index.js
var require_breadcrumbs = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/breadcrumbs/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Breadcrumbs = Breadcrumbs;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _Link = require_Link();
    var _excluded = ["items", "className", "collapseOnMobile"];
    var _excluded2 = ["href", "to", "reactListKey", "children"];
    function Breadcrumbs(props) {
      var items = props.items, className = props.className, collapseOnMobile = props.collapseOnMobile, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var breadcrumbs = items ? items.map(function(item, index) {
        var href = item.href, to = item.to, reactListKey = item.reactListKey, children = item.children, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
        return href || to ? _react["default"].createElement("li", {
          key: reactListKey || index,
          className: "govuk-breadcrumbs__list-item"
        }, _react["default"].createElement(_Link.Link, (0, _extends2["default"])({
          href,
          to,
          className: "govuk-breadcrumbs__link"
        }, itemAttributes), children)) : _react["default"].createElement("li", {
          key: reactListKey || index,
          className: "govuk-breadcrumbs__list-item",
          "aria-current": "page"
        }, children);
      }) : null;
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-breadcrumbs ".concat(className || "", " ").concat(collapseOnMobile ? "govuk-breadcrumbs--collapse-on-mobile" : "")
      }, attributes), _react["default"].createElement("ol", {
        className: "govuk-breadcrumbs__list"
      }, breadcrumbs));
    }
  }
});

// node_modules/@babel/runtime/helpers/toPrimitive.js
var require_toPrimitive = __commonJS({
  "node_modules/@babel/runtime/helpers/toPrimitive.js"(exports, module) {
    var _typeof2 = require_typeof()["default"];
    function toPrimitive(t, r) {
      if ("object" != _typeof2(t) || !t) return t;
      var e = t[Symbol.toPrimitive];
      if (void 0 !== e) {
        var i = e.call(t, r || "default");
        if ("object" != _typeof2(i)) return i;
        throw new TypeError("@@toPrimitive must return a primitive value.");
      }
      return ("string" === r ? String : Number)(t);
    }
    module.exports = toPrimitive, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/toPropertyKey.js
var require_toPropertyKey = __commonJS({
  "node_modules/@babel/runtime/helpers/toPropertyKey.js"(exports, module) {
    var _typeof2 = require_typeof()["default"];
    var toPrimitive = require_toPrimitive();
    function toPropertyKey(t) {
      var i = toPrimitive(t, "string");
      return "symbol" == _typeof2(i) ? i : i + "";
    }
    module.exports = toPropertyKey, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/@babel/runtime/helpers/defineProperty.js
var require_defineProperty = __commonJS({
  "node_modules/@babel/runtime/helpers/defineProperty.js"(exports, module) {
    var toPropertyKey = require_toPropertyKey();
    function _defineProperty(e, r, t) {
      return (r = toPropertyKey(r)) in e ? Object.defineProperty(e, r, {
        value: t,
        enumerable: true,
        configurable: true,
        writable: true
      }) : e[r] = t, e;
    }
    module.exports = _defineProperty, module.exports.__esModule = true, module.exports["default"] = module.exports;
  }
});

// node_modules/govuk-react-jsx/govuk/components/button/index.js
var require_button = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/button/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Button = void 0;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _defineProperty2 = _interopRequireDefault(require_defineProperty());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _Link = require_Link();
    var _excluded = ["element", "href", "to", "isStartButton", "disabled", "className", "preventDoubleClick", "name", "type", "children"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function ownKeys(object, enumerableOnly) {
      var keys = Object.keys(object);
      if (Object.getOwnPropertySymbols) {
        var symbols = Object.getOwnPropertySymbols(object);
        enumerableOnly && (symbols = symbols.filter(function(sym) {
          return Object.getOwnPropertyDescriptor(object, sym).enumerable;
        })), keys.push.apply(keys, symbols);
      }
      return keys;
    }
    function _objectSpread(target) {
      for (var i = 1; i < arguments.length; i++) {
        var source = null != arguments[i] ? arguments[i] : {};
        i % 2 ? ownKeys(Object(source), true).forEach(function(key) {
          (0, _defineProperty2["default"])(target, key, source[key]);
        }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)) : ownKeys(Object(source)).forEach(function(key) {
          Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));
        });
      }
      return target;
    }
    var Button = _react["default"].forwardRef(function(props, ref) {
      var element = props.element, href = props.href, to = props.to, isStartButton = props.isStartButton, disabled = props.disabled, className = props.className, preventDoubleClick = props.preventDoubleClick, name = props.name, type = props.type, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var buttonRef = ref || _react["default"].createRef();
      var el = "";
      var buttonAttributes = _objectSpread(_objectSpread({
        name,
        type
      }, attributes), {}, {
        "data-module": "govuk-button"
      });
      var button;
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, ButtonJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-button" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./button-6JEV6RSB.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  ButtonJS = _yield$import["default"];
                  if (buttonRef.current) {
                    new ButtonJS(buttonRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [buttonRef]);
      if (element) {
        el = element;
      } else if (href || to) {
        el = "a";
      } else {
        el = "button";
      }
      var iconHtml;
      if (isStartButton) {
        iconHtml = _react["default"].createElement("svg", {
          className: "govuk-button__start-icon",
          xmlns: "http://www.w3.org/2000/svg",
          width: "17.5",
          height: "19",
          viewBox: "0 0 33 40",
          "aria-hidden": "true",
          focusable: "false"
        }, _react["default"].createElement("path", {
          fill: "currentColor",
          d: "M0 0h13l20 20-20 20H0l20-20z"
        }));
      }
      var commonAttributes = {
        className: "govuk-button ".concat(className || "").concat(disabled ? " govuk-button--disabled" : "", " ").concat(isStartButton ? "govuk-button--start" : ""),
        ref: buttonRef
      };
      if (preventDoubleClick) {
        buttonAttributes["data-prevent-double-click"] = preventDoubleClick;
      }
      if (disabled) {
        buttonAttributes = _objectSpread(_objectSpread({}, buttonAttributes), {}, {
          "aria-disabled": true,
          disabled: "disabled"
        });
      }
      if (el === "a") {
        var linkAttributes = _objectSpread(_objectSpread(_objectSpread({}, commonAttributes), {}, {
          role: "button",
          draggable: "false"
        }, attributes), {}, {
          "data-module": "govuk-button",
          href,
          to
        });
        button = _react["default"].createElement(_Link.Link, linkAttributes, children, iconHtml);
      } else if (el === "button") {
        button = // Disabling linting of button type, because the button _does_ have an explicit type
        // It is defined in the defaultProps of the component, which gets added
        // to the buttonAttributes. eslint fails to detect this, and so we need to
        // disable the linting rule
        //
        // eslint-disable-next-line react/button-has-type
        _react["default"].createElement("button", (0, _extends2["default"])({}, buttonAttributes, commonAttributes), children, iconHtml);
      } else if (el === "input") {
        if (!type) {
          buttonAttributes.type = "submit";
        }
        button = _react["default"].createElement("input", (0, _extends2["default"])({
          value: children
        }, buttonAttributes, commonAttributes));
      }
      return button;
    });
    exports.Button = Button;
    Button.displayName = "Button";
  }
});

// node_modules/govuk-react-jsx/govuk/components/character-count/index.js
var require_character_count = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/character-count/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.CharacterCount = void 0;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _ = require_govuk();
    var _excluded = ["id", "className", "maxlength", "threshold", "maxwords", "errorMessage", "countMessage"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    var CharacterCount = _react["default"].forwardRef(function(props, ref) {
      var id = props.id, className = props.className, maxlength = props.maxlength, threshold = props.threshold, maxwords = props.maxwords, errorMessage = props.errorMessage, countMessage = props.countMessage, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var characterCountRef = (0, _react.useRef)();
      var characterCountInfoId = "".concat(id, "-info");
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, CharacterCountJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-character-count" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./character-count-LUU4ZDX4.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  CharacterCountJS = _yield$import["default"];
                  if (characterCountRef.current) {
                    new CharacterCountJS(characterCountRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [characterCountRef]);
      return _react["default"].createElement("div", {
        className: "govuk-character-count",
        "data-module": "govuk-character-count",
        "data-maxlength": maxlength,
        "data-threshold": threshold,
        "data-maxwords": maxwords,
        ref: characterCountRef
      }, _react["default"].createElement(_.Textarea, (0, _extends2["default"])({
        id
      }, attributes, {
        errorMessage,
        className: "govuk-js-character-count ".concat(className || "").concat(errorMessage ? " govuk-textarea--error" : ""),
        "aria-describedby": characterCountInfoId,
        ref
      })), _react["default"].createElement(_.Hint, {
        id: characterCountInfoId,
        className: "govuk-hint govuk-character-count__message ".concat((countMessage === null || countMessage === void 0 ? void 0 : countMessage.className) || ""),
        "aria-live": "polite"
      }, "You can enter up to ", maxlength || maxwords, " ", maxwords ? "words" : "characters"));
    });
    exports.CharacterCount = CharacterCount;
    CharacterCount.displayName = "CharacterCount";
  }
});

// node_modules/govuk-react-jsx/utils/omitKey.js
var require_omitKey = __commonJS({
  "node_modules/govuk-react-jsx/utils/omitKey.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof3 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports["default"] = omit;
    var _typeof2 = _interopRequireDefault(require_typeof());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    function _toPropertyKey(arg) {
      var key = _toPrimitive(arg, "string");
      return _typeof3(key) === "symbol" ? key : String(key);
    }
    function _toPrimitive(input, hint) {
      if ((0, _typeof2["default"])(input) !== "object" || input === null) return input;
      var prim = input[Symbol.toPrimitive];
      if (prim !== void 0) {
        var res = prim.call(input, hint || "default");
        if ((0, _typeof2["default"])(res) !== "object") return res;
        throw new TypeError("@@toPrimitive must return a primitive value.");
      }
      return (hint === "string" ? String : Number)(input);
    }
    function omit(object, key) {
      var deletedKey = object[key], otherKeys = (0, _objectWithoutProperties2["default"])(object, [key].map(_toPropertyKey));
      return otherKeys;
    }
  }
});

// node_modules/govuk-react-jsx/utils/Boolean.js
var require_Boolean = __commonJS({
  "node_modules/govuk-react-jsx/utils/Boolean.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Boolean = Boolean2;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _defineProperty2 = _interopRequireDefault(require_defineProperty());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _govuk = require_govuk();
    var _omitKey = _interopRequireDefault(require_omitKey());
    var _excluded = ["className", "errorMessage", "fieldset", "formGroup", "hint", "idPrefix", "items", "controlType", "name", "onChange", "onBlur", "aria-describedby"];
    var _excluded2 = ["id", "children", "hint", "conditional", "behaviour", "label", "reactListKey"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function ownKeys(object, enumerableOnly) {
      var keys = Object.keys(object);
      if (Object.getOwnPropertySymbols) {
        var symbols = Object.getOwnPropertySymbols(object);
        enumerableOnly && (symbols = symbols.filter(function(sym) {
          return Object.getOwnPropertyDescriptor(object, sym).enumerable;
        })), keys.push.apply(keys, symbols);
      }
      return keys;
    }
    function _objectSpread(target) {
      for (var i = 1; i < arguments.length; i++) {
        var source = null != arguments[i] ? arguments[i] : {};
        i % 2 ? ownKeys(Object(source), true).forEach(function(key) {
          (0, _defineProperty2["default"])(target, key, source[key]);
        }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)) : ownKeys(Object(source)).forEach(function(key) {
          Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));
        });
      }
      return target;
    }
    function Boolean2(props) {
      var className = props.className, errorMessage = props.errorMessage, fieldset = props.fieldset, formGroup = props.formGroup, hint = props.hint, idPrefix = props.idPrefix, items = props.items, controlType = props.controlType, name = props.name, onChange = props.onChange, onBlur = props.onBlur, describedByProp = props["aria-describedby"], attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var controlRef = (0, _react.useRef)();
      var idPrefixValue = idPrefix || name;
      var describedBy = describedByProp || "";
      if (fieldset !== null && fieldset !== void 0 && fieldset["aria-describedby"]) {
        describedBy = fieldset["aria-describedby"];
      }
      var hintComponent;
      var errorMessageComponent;
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, RadiosJS, _yield$import2, CheckboxesJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  _context.t0 = controlType;
                  _context.next = _context.t0 === "radios" ? 3 : _context.t0 === "checkboxes" ? 10 : 17;
                  break;
                case 3:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 9;
                    break;
                  }
                  _context.next = 6;
                  return import(
                    /* webpackChunkName: "govuk-frontend-radios" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./radios-CC64YLX4.js"
                  );
                case 6:
                  _yield$import = _context.sent;
                  RadiosJS = _yield$import["default"];
                  if (controlRef.current) {
                    new RadiosJS(controlRef.current).init();
                  }
                case 9:
                  return _context.abrupt("break", 17);
                case 10:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 16;
                    break;
                  }
                  _context.next = 13;
                  return import(
                    /* webpackChunkName: "govuk-frontend-checkboxes" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./checkboxes-LWKI4AUF.js"
                  );
                case 13:
                  _yield$import2 = _context.sent;
                  CheckboxesJS = _yield$import2["default"];
                  if (controlRef.current) {
                    new CheckboxesJS(controlRef.current).init();
                  }
                case 16:
                  return _context.abrupt("break", 17);
                case 17:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [controlRef, controlType]);
      if (hint) {
        var hintId = "".concat(idPrefixValue, "-hint");
        describedBy += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_govuk.Hint, (0, _extends2["default"])({}, hint, {
          id: hintId
        }));
      }
      var hasFieldset = !!fieldset;
      if (errorMessage) {
        var errorId = "".concat(idPrefixValue, "-error");
        describedBy += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_govuk.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      var innerHtml = _react["default"].createElement(_react["default"].Fragment, null, hintComponent, errorMessageComponent, _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-".concat(controlType, " ").concat(className || "")
      }, attributes, {
        ref: controlRef,
        "data-module": "govuk-".concat(controlType)
      }), items && items.map(function(item, index) {
        if (!item) {
          return null;
        }
        if (item.behaviour === "exclusive") {
          delete item.behaviour;
        }
        var id = item.id, children = item.children, itemHint = item.hint, itemConditional = item.conditional, behaviour = item.behaviour, label = item.label, reactListKey = item.reactListKey, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
        var idSuffix = "-".concat(index + 1);
        var idValue = id || "".concat(idPrefixValue).concat(index === 0 ? "" : idSuffix);
        var nameValue = item.name ? item.name : name;
        var conditionalId = itemConditional !== null && itemConditional !== void 0 && itemConditional.children ? "conditional-".concat(idValue) : null;
        var itemHintId = "".concat(idValue, "-item-hint");
        var itemDescribedBy = "";
        if (controlType === "checkboxes" && !hasFieldset) {
          itemDescribedBy = describedBy;
        }
        if (itemHint) {
          itemDescribedBy += " ".concat(itemHintId);
        }
        if (item.divider) {
          return _react["default"].createElement("div", {
            key: reactListKey || index,
            className: "govuk-".concat(controlType, "__divider")
          }, item.divider);
        }
        return _react["default"].createElement(_react["default"].Fragment, {
          key: reactListKey || index
        }, _react["default"].createElement("div", {
          className: "govuk-".concat(controlType, "__item")
        }, _react["default"].createElement("input", (0, _extends2["default"])({
          className: "govuk-".concat(controlType, "__input"),
          id: idValue,
          name: nameValue,
          type: controlType === "radios" ? "radio" : "checkbox",
          "data-aria-controls": conditionalId,
          "aria-describedby": itemDescribedBy || null,
          onChange,
          onBlur,
          "data-behaviour": behaviour
        }, itemAttributes)), _react["default"].createElement(_govuk.Label, _objectSpread(_objectSpread({}, label), {}, {
          className: "govuk-".concat(controlType, "__label ").concat((label === null || label === void 0 ? void 0 : label.className) || ""),
          htmlFor: idValue,
          isPageHeading: false
        }), children), itemHint ? _react["default"].createElement(_govuk.Hint, (0, _extends2["default"])({}, _objectSpread(_objectSpread({}, itemHint), {}, {
          className: "govuk-".concat(controlType, "__hint ").concat(itemHint.className || "")
        }), {
          id: itemHintId
        })) : ""), itemConditional !== null && itemConditional !== void 0 && itemConditional.children ? _react["default"].createElement("div", {
          className: "govuk-".concat(controlType, "__conditional ").concat(item.checked ? "" : "govuk-".concat(controlType, "__conditional--hidden")),
          id: conditionalId
        }, itemConditional.children) : "");
      })));
      return _react["default"].createElement("div", {
        className: "govuk-form-group".concat(errorMessage ? " govuk-form-group--error" : "", " ").concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "")
      }, hasFieldset ? _react["default"].createElement(_govuk.Fieldset, (0, _extends2["default"])({}, (0, _omitKey["default"])(fieldset, "role"), {
        "aria-describedby": describedBy.trim() || null
      }), innerHtml) : innerHtml);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/checkboxes/index.js
var require_checkboxes = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/checkboxes/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Checkboxes = Checkboxes;
    var _extends2 = _interopRequireDefault(require_extends());
    var _react = _interopRequireDefault(require_react());
    var _Boolean = require_Boolean();
    function Checkboxes(props) {
      return _react["default"].createElement(_Boolean.Boolean, (0, _extends2["default"])({}, props, {
        controlType: "checkboxes"
      }));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/cookie-banner/index.js
var require_cookie_banner = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/cookie-banner/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.CookieBanner = CookieBanner;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _button = require_button();
    var _Link = require_Link();
    var _excluded = ["className", "messages"];
    var _excluded2 = ["headingChildren", "children", "actions", "className"];
    var _excluded3 = ["className"];
    function CookieBanner(props) {
      var className = props.className, messages = props.messages, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-cookie-banner ".concat(className || ""),
        "data-nosnippet": true,
        role: "region"
      }, attributes), messages.map(function(message) {
        var headingChildren = message.headingChildren, children = message.children, actions = message.actions, messageClassName = message.className, messageAttributes = (0, _objectWithoutProperties2["default"])(message, _excluded2);
        return _react["default"].createElement("div", (0, _extends2["default"])({
          className: "govuk-cookie-banner__message govuk-width-container ".concat(messageClassName || "")
        }, messageAttributes), _react["default"].createElement("div", {
          className: "govuk-grid-row"
        }, _react["default"].createElement("div", {
          className: "govuk-grid-column-two-thirds"
        }, headingChildren ? _react["default"].createElement("h2", {
          className: "govuk-cookie-banner__heading govuk-heading-m"
        }, headingChildren) : null, _react["default"].createElement("div", {
          className: "govuk-cookie-banner__content"
        }, children ? typeof children === "string" ? _react["default"].createElement("p", {
          className: "govuk-body"
        }, children) : children : null))), actions ? _react["default"].createElement("div", {
          className: "govuk-button-group"
        }, actions.map(function(action) {
          var actionClassName = action.className, actionAttributes = (0, _objectWithoutProperties2["default"])(action, _excluded3);
          return action.href || action.to ? action.type === "button" ? _react["default"].createElement(_button.Button, action) : _react["default"].createElement(_Link.Link, (0, _extends2["default"])({}, actionAttributes, {
            className: "govuk-link ".concat(actionClassName || "")
          })) : _react["default"].createElement(_button.Button, action);
        })) : null);
      }));
    }
    CookieBanner.defaultProps = {
      "aria-label": "Cookie banner"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/date-input/index.js
var require_date_input = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/date-input/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.DateInput = DateInput;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "errorMessage", "fieldset", "formGroup", "hint", "id", "items", "namePrefix", "onChange"];
    var _excluded2 = ["name", "inputMode", "label", "reactListKey", "id", "className", "pattern"];
    function DateInput(props) {
      var className = props.className, errorMessage = props.errorMessage, fieldset = props.fieldset, formGroup = props.formGroup, hint = props.hint, id = props.id, items = props.items, namePrefix = props.namePrefix, onChange = props.onChange, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var describedBy = fieldset !== null && fieldset !== void 0 && fieldset["aria-describedby"] ? fieldset["aria-describedby"] : "";
      var hintComponent;
      var errorMessageComponent;
      var dateInputItems = [];
      if (hint) {
        var hintId = "".concat(id, "-hint");
        describedBy += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_.Hint, (0, _extends2["default"])({}, hint, {
          id: hintId
        }));
      }
      if (errorMessage) {
        var errorId = id ? "".concat(id, "-error") : "";
        describedBy += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      if (items && items.length > 0) {
        dateInputItems = items;
      } else {
        dateInputItems = [{
          name: "day",
          className: "govuk-input--width-2",
          type: "text"
        }, {
          name: "month",
          className: "govuk-input--width-2",
          type: "text"
        }, {
          name: "year",
          className: "govuk-input--width-4",
          type: "text"
        }];
      }
      var itemComponents = dateInputItems.filter(function(item) {
        return item;
      }).map(function(item, index) {
        var itemName = item.name, itemInputMode = item.inputMode, itemLabel = item.label, itemReactListKey = item.reactListKey, itemId = item.id, itemClassName = item.className, itemPattern = item.pattern, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
        return _react["default"].createElement("div", {
          key: itemReactListKey || index,
          className: "govuk-date-input__item"
        }, _react["default"].createElement(_.Input, (0, _extends2["default"])({
          onChange
        }, itemAttributes, {
          label: {
            children: itemLabel || itemName.charAt(0).toUpperCase() + itemName.slice(1),
            className: "govuk-date-input__label"
          },
          id: itemId || "".concat(id, "-").concat(itemName),
          className: "govuk-date-input__input ".concat(itemClassName || ""),
          name: namePrefix ? "".concat(namePrefix, "-").concat(itemName) : itemName,
          type: "text",
          inputMode: itemInputMode || "numeric",
          pattern: itemPattern || "[0-9]*"
        })));
      });
      var innerHtml = _react["default"].createElement(_react["default"].Fragment, null, hintComponent, errorMessageComponent, _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-date-input ".concat(className || "")
      }, attributes, {
        id
      }), itemComponents));
      return _react["default"].createElement("div", {
        className: "govuk-form-group".concat(errorMessage ? " govuk-form-group--error" : "", " ").concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "")
      }, fieldset ? _react["default"].createElement(_.Fieldset, (0, _extends2["default"])({}, fieldset, {
        "aria-describedby": describedBy || null,
        role: "group"
      }), innerHtml) : _react["default"].createElement(_react["default"].Fragment, null, innerHtml));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/details/index.js
var require_details = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/details/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Details = Details;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "children", "summaryChildren"];
    function Details(props) {
      var className = props.className, children = props.children, summaryChildren = props.summaryChildren, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("details", (0, _extends2["default"])({
        className: "govuk-details ".concat(className || "")
      }, attributes, {
        "data-module": "govuk-details"
      }), _react["default"].createElement("summary", {
        className: "govuk-details__summary"
      }, _react["default"].createElement("span", {
        className: "govuk-details__summary-text"
      }, summaryChildren)), _react["default"].createElement("div", {
        className: "govuk-details__text"
      }, children));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/error-message/index.js
var require_error_message = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/error-message/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.ErrorMessage = ErrorMessage;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "children", "visuallyHiddenText"];
    function ErrorMessage(props) {
      var className = props.className, children = props.children, visuallyHiddenText = props.visuallyHiddenText, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var visuallyHiddenTextComponent;
      if (visuallyHiddenText) {
        visuallyHiddenTextComponent = _react["default"].createElement("span", {
          className: "govuk-visually-hidden"
        }, visuallyHiddenText, ": ");
      }
      return _react["default"].createElement("p", (0, _extends2["default"])({
        className: "govuk-error-message ".concat(className || "")
      }, attributes), visuallyHiddenTextComponent, children);
    }
    ErrorMessage.defaultProps = {
      visuallyHiddenText: "Error"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/error-summary/index.js
var require_error_summary = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/error-summary/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.ErrorSummary = void 0;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _excluded = ["className", "descriptionChildren", "errorList", "titleChildren", "disableAutoFocus"];
    var _excluded2 = ["reactListKey", "children", "href"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    var defaultRef = _react["default"].createRef();
    var ErrorSummary = _react["default"].forwardRef(function(props, ref) {
      var className = props.className, descriptionChildren = props.descriptionChildren, errorList = props.errorList, titleChildren = props.titleChildren, disableAutoFocus = props.disableAutoFocus, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var errorSummaryRef = ref || defaultRef;
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, ErrorSummaryJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-error-summary" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./error-summary-6YHHNDXX.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  ErrorSummaryJS = _yield$import["default"];
                  if (errorSummaryRef.current) {
                    errorSummaryRef.current.addEventListener("click", ErrorSummaryJS.prototype.handleClick.bind(ErrorSummaryJS.prototype));
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [errorSummaryRef]);
      var description;
      if (descriptionChildren) {
        description = _react["default"].createElement("p", null, descriptionChildren);
      }
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-error-summary ".concat(className || ""),
        "aria-labelledby": "error-summary-title",
        role: "alert",
        "data-disable-auto-focus": disableAutoFocus ? "true" : null
      }, attributes, {
        "data-module": "govuk-error-summary",
        ref: errorSummaryRef
      }), _react["default"].createElement("h2", {
        className: "govuk-error-summary__title",
        id: "error-summary-title"
      }, titleChildren), _react["default"].createElement("div", {
        className: "govuk-error-summary__body"
      }, description, _react["default"].createElement("ul", {
        className: "govuk-list govuk-error-summary__list"
      }, errorList ? errorList.map(function(error, index) {
        var reactListKey = error.reactListKey, children = error.children, href = error.href, errorAttributes = (0, _objectWithoutProperties2["default"])(error, _excluded2);
        return _react["default"].createElement("li", {
          key: reactListKey || index
        }, href ? _react["default"].createElement("a", (0, _extends2["default"])({}, errorAttributes, {
          href
        }), children) : _react["default"].createElement(_react["default"].Fragment, null, children));
      }) : null)));
    });
    exports.ErrorSummary = ErrorSummary;
    ErrorSummary.defaultProps = {
      titleChildren: "There is a problem"
    };
    ErrorSummary.displayName = "ErrorSummary";
  }
});

// node_modules/govuk-react-jsx/govuk/components/fieldset/index.js
var require_fieldset = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/fieldset/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Fieldset = Fieldset;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["legend", "className", "children"];
    function Fieldset(props) {
      var legend = props.legend, className = props.className, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var legendComponent;
      if (legend && legend.children) {
        legendComponent = _react["default"].createElement("legend", {
          className: "govuk-fieldset__legend ".concat(legend.className || "")
        }, legend.isPageHeading ? _react["default"].createElement("h1", {
          className: "govuk-fieldset__heading"
        }, legend.children) : legend.children);
      }
      return _react["default"].createElement("fieldset", (0, _extends2["default"])({
        className: "govuk-fieldset ".concat(className || "")
      }, attributes), legendComponent, children);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/file-upload/index.js
var require_file_upload = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/file-upload/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.FileUpload = void 0;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "errorMessage", "formGroup", "hint", "label", "aria-describedby", "id"];
    var FileUpload = _react["default"].forwardRef(function(props, ref) {
      var className = props.className, errorMessage = props.errorMessage, formGroup = props.formGroup, hint = props.hint, label = props.label, describedBy = props["aria-describedby"], id = props.id, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var hintComponent;
      var errorMessageComponent;
      var describedByValue = describedBy || "";
      if (hint) {
        var hintId = "".concat(props.id, "-hint");
        describedByValue += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_.Hint, (0, _extends2["default"])({}, props.hint, {
          id: hintId
        }));
      }
      if (errorMessage) {
        var errorId = "".concat(id, "-error");
        describedByValue += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      return _react["default"].createElement("div", {
        className: "govuk-form-group".concat(errorMessage ? " govuk-form-group--error" : "", " ").concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "")
      }, _react["default"].createElement(_.Label, (0, _extends2["default"])({}, label, {
        htmlFor: id
      })), hintComponent, errorMessageComponent, _react["default"].createElement("input", (0, _extends2["default"])({}, attributes, {
        id,
        ref,
        className: "govuk-file-upload ".concat(className || "").concat(errorMessage ? " govuk-file-upload--error" : ""),
        type: "file",
        "aria-describedby": describedByValue || null
      })));
    });
    exports.FileUpload = FileUpload;
    FileUpload.displayName = "FileUpload";
  }
});

// node_modules/govuk-react-jsx/govuk/components/footer/index.js
var require_footer = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/footer/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Footer = Footer;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _Link = require_Link();
    var _excluded = ["className", "containerClassName", "meta", "navigation"];
    var _excluded2 = ["className", "children", "reactListKey"];
    var _excluded3 = ["className", "children", "reactListKey"];
    function Footer(props) {
      var className = props.className, containerClassName = props.containerClassName, meta = props.meta, navigation = props.navigation, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var navigationComponent;
      var metaComponent;
      if (navigation && navigation.length > 0) {
        navigationComponent = _react["default"].createElement(_react["default"].Fragment, null, _react["default"].createElement("div", {
          className: "govuk-footer__navigation"
        }, navigation.map(function(nav, navIndex) {
          return _react["default"].createElement("div", {
            className: "govuk-footer__section govuk-grid-column-".concat(nav.width ? nav.width : "full"),
            key: nav.reactListKey || navIndex
          }, _react["default"].createElement("h2", {
            className: "govuk-footer__heading govuk-heading-m"
          }, nav.title), nav.items && nav.items.length > 0 ? _react["default"].createElement("ul", {
            className: "govuk-footer__list ".concat(nav.columns ? "govuk-footer__list--columns-".concat(nav.columns) : "")
          }, nav.items.map(function(item, index) {
            var itemClassName = item.className, itemChildren = item.children, reactListKey = item.reactListKey, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
            return _react["default"].createElement(_react["default"].Fragment, {
              key: reactListKey || index
            }, (item.href || item.to) && itemChildren && _react["default"].createElement("li", {
              className: "govuk-footer__list-item"
            }, _react["default"].createElement(_Link.Link, (0, _extends2["default"])({
              className: "govuk-footer__link ".concat(itemClassName || "")
            }, itemAttributes), itemChildren)));
          })) : null);
        })), _react["default"].createElement("hr", {
          className: "govuk-footer__section-break"
        }));
      }
      if (meta) {
        metaComponent = _react["default"].createElement(_react["default"].Fragment, null, _react["default"].createElement("h2", {
          className: "govuk-visually-hidden"
        }, meta.visuallyHiddenTitle ? meta.visuallyHiddenTitle : "Support links"), meta.items && meta.items.length > 0 ? _react["default"].createElement(_react["default"].Fragment, null, _react["default"].createElement("ul", {
          className: "govuk-footer__inline-list"
        }, meta.items.map(function(item, index) {
          var itemClassName = item.className, itemChildren = item.children, reactListKey = item.reactListKey, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded3);
          return _react["default"].createElement("li", {
            className: "govuk-footer__inline-list-item",
            key: reactListKey || index
          }, _react["default"].createElement(_Link.Link, (0, _extends2["default"])({
            className: "govuk-footer__link ".concat(itemClassName || "")
          }, itemAttributes), itemChildren));
        }))) : null, meta.children ? _react["default"].createElement("div", {
          className: "govuk-footer__meta-custom"
        }, meta.children) : null);
      }
      return _react["default"].createElement("footer", (0, _extends2["default"])({
        className: "govuk-footer ".concat(className || ""),
        role: "contentinfo"
      }, attributes), _react["default"].createElement("div", {
        className: "govuk-width-container ".concat(containerClassName || "")
      }, navigationComponent, _react["default"].createElement("div", {
        className: "govuk-footer__meta"
      }, _react["default"].createElement("div", {
        className: "govuk-footer__meta-item govuk-footer__meta-item--grow"
      }, metaComponent, _react["default"].createElement("svg", {
        "aria-hidden": "true",
        focusable: "false",
        className: "govuk-footer__licence-logo",
        xmlns: "http://www.w3.org/2000/svg",
        viewBox: "0 0 483.2 195.7",
        height: "17",
        width: "41"
      }, _react["default"].createElement("path", {
        fill: "currentColor",
        d: "M421.5 142.8V.1l-50.7 32.3v161.1h112.4v-50.7zm-122.3-9.6A47.12 47.12 0 0 1 221 97.8c0-26 21.1-47.1 47.1-47.1 16.7 0 31.4 8.7 39.7 21.8l42.7-27.2A97.63 97.63 0 0 0 268.1 0c-36.5 0-68.3 20.1-85.1 49.7A98 98 0 0 0 97.8 0C43.9 0 0 43.9 0 97.8s43.9 97.8 97.8 97.8c36.5 0 68.3-20.1 85.1-49.7a97.76 97.76 0 0 0 149.6 25.4l19.4 22.2h3v-87.8h-80l24.3 27.5zM97.8 145c-26 0-47.1-21.1-47.1-47.1s21.1-47.1 47.1-47.1 47.2 21 47.2 47S123.8 145 97.8 145"
      })), _react["default"].createElement("span", {
        className: "govuk-footer__licence-description"
      }, "All content is available under the", " ", _react["default"].createElement("a", {
        className: "govuk-footer__link",
        href: "https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/",
        rel: "license"
      }, "Open Government Licence v3.0"), ", except where otherwise stated")), _react["default"].createElement("div", {
        className: "govuk-footer__meta-item"
      }, _react["default"].createElement("a", {
        className: "govuk-footer__link govuk-footer__copyright-logo",
        href: "https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/"
      }, "© Crown copyright")))));
    }
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-logotype-crown.png
var govuk_logotype_crown_exports = {};
__export(govuk_logotype_crown_exports, {
  default: () => default2
});
import { default as default2 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-logotype-crown.png";
import * as govuk_logotype_crown_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-logotype-crown.png";
var init_govuk_logotype_crown = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-logotype-crown.png"() {
    __reExport(govuk_logotype_crown_exports, govuk_logotype_crown_star);
  }
});

// node_modules/govuk-react-jsx/govuk/components/header/index.js
var require_header = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/header/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Header = Header;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _govukLogotypeCrown = _interopRequireDefault((init_govuk_logotype_crown(), __toCommonJS(govuk_logotype_crown_exports)));
    var _Link = require_Link();
    var _excluded = ["className", "containerClassName", "homepageUrlHref", "homepageUrlTo", "navigation", "navigationClassName", "productName", "serviceName", "serviceUrlHref", "serviceUrlTo", "navigationLabel", "menuButtonLabel", "assetsPath"];
    var _excluded2 = ["active", "className", "children", "reactListKey"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function Header(props) {
      var className = props.className, containerClassName = props.containerClassName, homepageUrlHref = props.homepageUrlHref, homepageUrlTo = props.homepageUrlTo, navigation = props.navigation, navigationClassName = props.navigationClassName, productName = props.productName, serviceName = props.serviceName, serviceUrlHref = props.serviceUrlHref, serviceUrlTo = props.serviceUrlTo, navigationLabel = props.navigationLabel, menuButtonLabel = props.menuButtonLabel, assetsPath = props.assetsPath, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var headerRef = (0, _react.useRef)();
      var productNameComponent;
      var navigationComponent;
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, HeaderJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-header" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./header-I6UE5UIM.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  HeaderJS = _yield$import["default"];
                  if (headerRef.current) {
                    new HeaderJS(headerRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [headerRef]);
      if (productName) {
        productNameComponent = _react["default"].createElement("span", {
          className: "govuk-header__product-name"
        }, productName);
      }
      if (serviceName || navigation) {
        navigationComponent = _react["default"].createElement("div", {
          className: "govuk-header__content"
        }, serviceName ? _react["default"].createElement(_Link.Link, {
          href: serviceUrlHref,
          to: serviceUrlTo,
          className: "govuk-header__link govuk-header__link--service-name"
        }, serviceName) : null, navigation ? _react["default"].createElement(_react["default"].Fragment, null, _react["default"].createElement("nav", {
          className: "govuk-header__navigation ".concat(navigationClassName || ""),
          "aria-label": navigationLabel
        }, _react["default"].createElement("button", {
          type: "button",
          className: "govuk-header__menu-button govuk-js-header-toggle",
          "aria-controls": "navigation",
          "aria-label": menuButtonLabel
        }, "Menu"), _react["default"].createElement("ul", {
          id: "navigation",
          className: "govuk-header__navigation-list"
        }, navigation.map(function(item, index) {
          var itemActive = item.active, itemClassName = item.className, itemChildren = item.children, reactListKey = item.reactListKey, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
          return itemChildren ? _react["default"].createElement("li", {
            key: reactListKey || index,
            className: "govuk-header__navigation-item".concat(itemActive ? " govuk-header__navigation-item--active" : "")
          }, item.href || item.to ? _react["default"].createElement(_Link.Link, (0, _extends2["default"])({
            className: "govuk-header__link ".concat(itemClassName || "")
          }, itemAttributes), itemChildren) : itemChildren) : null;
        })))) : null);
      }
      return _react["default"].createElement("header", (0, _extends2["default"])({
        className: "govuk-header ".concat(className || ""),
        role: "banner",
        "data-module": "govuk-header"
      }, attributes, {
        ref: headerRef
      }), _react["default"].createElement("div", {
        className: "govuk-header__container ".concat(containerClassName)
      }, _react["default"].createElement("div", {
        className: "govuk-header__logo"
      }, _react["default"].createElement(_Link.Link, {
        to: homepageUrlTo,
        href: homepageUrlHref,
        className: "govuk-header__link govuk-header__link--homepage"
      }, _react["default"].createElement("span", {
        className: "govuk-header__logotype",
        dangerouslySetInnerHTML: {
          __html: '\n                  <!--[if gt IE 8]><!-->\n                    <svg\n                      aria-hidden="true"\n                      focusable="false"\n                      class="govuk-header__logotype-crown"\n                      xmlns="http://www.w3.org/2000/svg"\n                      viewBox="0 0 132 97"\n                      height="30"\n                      width="36"\n                    >\n                      <path\n                        fill="currentColor"\n                        fillRule="evenodd"\n                        d="M25 30.2c3.5 1.5 7.7-.2 9.1-3.7 1.5-3.6-.2-7.8-3.9-9.2-3.6-1.4-7.6.3-9.1 3.9-1.4 3.5.3 7.5 3.9 9zM9 39.5c3.6 1.5 7.8-.2 9.2-3.7 1.5-3.6-.2-7.8-3.9-9.1-3.6-1.5-7.6.2-9.1 3.8-1.4 3.5.3 7.5 3.8 9zM4.4 57.2c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.5-1.5-7.6.3-9.1 3.8-1.4 3.5.3 7.6 3.9 9.1zm38.3-21.4c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.6-1.5-7.6.3-9.1 3.8-1.3 3.6.4 7.7 3.9 9.1zm64.4-5.6c-3.6 1.5-7.8-.2-9.1-3.7-1.5-3.6.2-7.8 3.8-9.2 3.6-1.4 7.7.3 9.2 3.9 1.3 3.5-.4 7.5-3.9 9zm15.9 9.3c-3.6 1.5-7.7-.2-9.1-3.7-1.5-3.6.2-7.8 3.7-9.1 3.6-1.5 7.7.2 9.2 3.8 1.5 3.5-.3 7.5-3.8 9zm4.7 17.7c-3.6 1.5-7.8-.2-9.2-3.8-1.5-3.6.2-7.7 3.9-9.1 3.6-1.5 7.7.3 9.2 3.8 1.3 3.5-.4 7.6-3.9 9.1zM89.3 35.8c-3.6 1.5-7.8-.2-9.2-3.8-1.4-3.6.2-7.7 3.9-9.1 3.6-1.5 7.7.3 9.2 3.8 1.4 3.6-.3 7.7-3.9 9.1zM69.7 17.7l8.9 4.7V9.3l-8.9 2.8c-.2-.3-.5-.6-.9-.9L72.4 0H59.6l3.5 11.2c-.3.3-.6.5-.9.9l-8.8-2.8v13.1l8.8-4.7c.3.3.6.7.9.9l-5 15.4v.1c-.2.8-.4 1.6-.4 2.4 0 4.1 3.1 7.5 7 8.1h.2c.3 0 .7.1 1 .1.4 0 .7 0 1-.1h.2c4-.6 7.1-4.1 7.1-8.1 0-.8-.1-1.7-.4-2.4V34l-5.1-15.4c.4-.2.7-.6 1-.9zM66 92.8c16.9 0 32.8 1.1 47.1 3.2 4-16.9 8.9-26.7 14-33.5l-9.6-3.4c1 4.9 1.1 7.2 0 10.2-1.5-1.4-3-4.3-4.2-8.7L108.6 76c2.8-2 5-3.2 7.5-3.3-4.4 9.4-10 11.9-13.6 11.2-4.3-.8-6.3-4.6-5.6-7.9 1-4.7 5.7-5.9 8-.5 4.3-8.7-3-11.4-7.6-8.8 7.1-7.2 7.9-13.5 2.1-21.1-8 6.1-8.1 12.3-4.5 20.8-4.7-5.4-12.1-2.5-9.5 6.2 3.4-5.2 7.9-2 7.2 3.1-.6 4.3-6.4 7.8-13.5 7.2-10.3-.9-10.9-8-11.2-13.8 2.5-.5 7.1 1.8 11 7.3L80.2 60c-4.1 4.4-8 5.3-12.3 5.4 1.4-4.4 8-11.6 8-11.6H55.5s6.4 7.2 7.9 11.6c-4.2-.1-8-1-12.3-5.4l1.4 16.4c3.9-5.5 8.5-7.7 10.9-7.3-.3 5.8-.9 12.8-11.1 13.8-7.2.6-12.9-2.9-13.5-7.2-.7-5 3.8-8.3 7.1-3.1 2.7-8.7-4.6-11.6-9.4-6.2 3.7-8.5 3.6-14.7-4.6-20.8-5.8 7.6-5 13.9 2.2 21.1-4.7-2.6-11.9.1-7.7 8.8 2.3-5.5 7.1-4.2 8.1.5.7 3.3-1.3 7.1-5.7 7.9-3.5.7-9-1.8-13.5-11.2 2.5.1 4.7 1.3 7.5 3.3l-4.7-15.4c-1.2 4.4-2.7 7.2-4.3 8.7-1.1-3-.9-5.3 0-10.2l-9.5 3.4c5 6.9 9.9 16.7 14 33.5 14.8-2.1 30.8-3.2 47.7-3.2z"\n                      />\n                    </svg>\n                  <!--<![endif]-->\n                  <!--[if IE 8]>\n                    <img src="'.concat(_govukLogotypeCrown["default"], '" class="govuk-header__logotype-crown-fallback-image" width="36" height="32" />\n                  <![endif]-->\n                  <span class="govuk-header__logotype-text">GOV.UK</span>\n              ')
        }
      }), productNameComponent)), navigationComponent));
    }
    Header.defaultProps = {
      homepageUrlHref: "/",
      containerClassName: "govuk-width-container",
      navigationLabel: "Menu",
      menuButtonLabel: "Show or hide menu"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/hint/index.js
var require_hint = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/hint/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Hint = Hint;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "children"];
    function Hint(props) {
      var className = props.className, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-hint ".concat(className || "")
      }, attributes), children);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/input/index.js
var require_input = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/input/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Input = void 0;
    var _defineProperty2 = _interopRequireDefault(require_defineProperty());
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "aria-describedby", "errorMessage", "formGroup", "hint", "label", "name", "id", "prefix", "suffix"];
    function ownKeys(object, enumerableOnly) {
      var keys = Object.keys(object);
      if (Object.getOwnPropertySymbols) {
        var symbols = Object.getOwnPropertySymbols(object);
        enumerableOnly && (symbols = symbols.filter(function(sym) {
          return Object.getOwnPropertyDescriptor(object, sym).enumerable;
        })), keys.push.apply(keys, symbols);
      }
      return keys;
    }
    function _objectSpread(target) {
      for (var i = 1; i < arguments.length; i++) {
        var source = null != arguments[i] ? arguments[i] : {};
        i % 2 ? ownKeys(Object(source), true).forEach(function(key) {
          (0, _defineProperty2["default"])(target, key, source[key]);
        }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)) : ownKeys(Object(source)).forEach(function(key) {
          Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));
        });
      }
      return target;
    }
    var Input = _react["default"].forwardRef(function(props, ref) {
      var className = props.className, describedBy = props["aria-describedby"], errorMessage = props.errorMessage, formGroup = props.formGroup, hint = props.hint, label = props.label, name = props.name, id = props.id, prefix = props.prefix, suffix = props.suffix, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var describedByValue = describedBy || "";
      var hintComponent;
      var errorMessageComponent;
      if (hint) {
        var hintId = "".concat(id, "-hint");
        describedByValue += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_.Hint, (0, _extends2["default"])({}, hint, {
          id: hintId
        }));
      }
      if (errorMessage) {
        var errorId = id ? "".concat(id, "-error") : "";
        describedByValue += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      var input = _react["default"].createElement("input", (0, _extends2["default"])({
        ref,
        id,
        className: "govuk-input ".concat(className || "", " ").concat(errorMessage ? " govuk-input--error" : ""),
        name: name || id,
        "aria-describedby": describedByValue || null
      }, attributes));
      return _react["default"].createElement("div", {
        className: "govuk-form-group ".concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "", " ").concat(errorMessage ? "govuk-form-group--error" : "", " ")
      }, _react["default"].createElement(_.Label, (0, _extends2["default"])({}, label, {
        htmlFor: id
      })), hintComponent, errorMessageComponent, prefix || suffix ? _react["default"].createElement("div", {
        className: "govuk-input__wrapper"
      }, prefix ? _react["default"].createElement("div", (0, _extends2["default"])({
        "aria-hidden": "true"
      }, _objectSpread(_objectSpread({}, prefix), {}, {
        className: "govuk-input__prefix ".concat(prefix.className ? prefix.className : "")
      }))) : null, input, suffix ? _react["default"].createElement("div", (0, _extends2["default"])({
        "aria-hidden": "true"
      }, _objectSpread(_objectSpread({}, suffix), {}, {
        className: "govuk-input__suffix ".concat(suffix.className ? suffix.className : "")
      }))) : null) : input);
    });
    exports.Input = Input;
    Input.displayName = "Input";
    Input.defaultProps = {
      type: "text"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/inset-text/index.js
var require_inset_text = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/inset-text/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.InsetText = InsetText;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "children"];
    function InsetText(props) {
      var className = props.className, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-inset-text ".concat(className || "")
      }, attributes), children);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/label/index.js
var require_label = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/label/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Label = Label;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "htmlFor", "children", "isPageHeading"];
    function Label(props) {
      var className = props.className, htmlFor = props.htmlFor, children = props.children, isPageHeading = props.isPageHeading, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      if (!children) {
        return null;
      }
      var label = (
        // Stop eslint flagging the for/id combination as an error. It is failing due to the way the
        // input and label are located in different components and so it cannot track the association
        //
        // eslint-disable-next-line jsx-a11y/label-has-for
        _react["default"].createElement("label", (0, _extends2["default"])({
          className: "govuk-label ".concat(className || "")
        }, attributes, {
          htmlFor
        }), children)
      );
      if (isPageHeading === true) {
        return _react["default"].createElement("h1", {
          className: "govuk-label-wrapper"
        }, label);
      }
      return label;
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/notification-banner/index.js
var require_notification_banner = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/notification-banner/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.NotificationBanner = NotificationBanner;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _excluded = ["type", "titleChildren", "titleHeadingLevel", "children", "className", "titleId", "role", "disableAutoFocus"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function NotificationBanner(props) {
      var type = props.type, titleChildren = props.titleChildren, titleHeadingLevel = props.titleHeadingLevel, children = props.children, className = props.className, titleId = props.titleId, role = props.role, disableAutoFocus = props.disableAutoFocus, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var notificationBannerRef = (0, _react.useRef)();
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, NotificationBannerJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-notification-banner" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./notification-banner-KHS2XSHZ.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  NotificationBannerJS = _yield$import["default"];
                  if (notificationBannerRef.current) {
                    new NotificationBannerJS(notificationBannerRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [notificationBannerRef]);
      var HeadingLevel = titleHeadingLevel ? "h".concat(titleHeadingLevel) : "h2";
      var successBanner = false;
      if (type === "success") {
        successBanner = true;
      }
      var typeClass = "";
      if (successBanner) {
        typeClass = "govuk-notification-banner--".concat(type);
      }
      var roleAttribute = "region";
      if (role) {
        roleAttribute = role;
      } else if (successBanner) {
        roleAttribute = "alert";
      }
      var title = titleChildren;
      if (!titleChildren) {
        if (successBanner) {
          title = "Success";
        } else {
          title = "Important";
        }
      }
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-notification-banner ".concat(typeClass, " ").concat(className || ""),
        role: roleAttribute,
        "aria-labelledby": titleId,
        "data-module": "govuk-notification-banner",
        ref: notificationBannerRef
      }, disableAutoFocus ? {
        "data-disable-auto-focus": "true"
      } : {}, attributes), _react["default"].createElement("div", {
        className: "govuk-notification-banner__header"
      }, _react["default"].createElement(HeadingLevel, {
        className: "govuk-notification-banner__title",
        id: titleId
      }, title)), _react["default"].createElement("div", {
        className: "govuk-notification-banner__content"
      }, typeof children === "string" ? _react["default"].createElement("p", {
        className: "govuk-notification-banner__heading"
      }, children) : children));
    }
    NotificationBanner.defaultProps = {
      titleId: "govuk-notification-banner-title"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/panel/index.js
var require_panel = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/panel/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Panel = Panel;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["headingLevel", "children", "className", "titleChildren"];
    function Panel(props) {
      var headingLevel = props.headingLevel, children = props.children, className = props.className, titleChildren = props.titleChildren, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var HeadingLevel = headingLevel ? "h".concat(headingLevel) : "h1";
      var innerHtml = children ? _react["default"].createElement("div", {
        className: "govuk-panel__body"
      }, children) : null;
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-panel govuk-panel--confirmation ".concat(className || "")
      }, attributes), _react["default"].createElement(HeadingLevel, {
        className: "govuk-panel__title"
      }, titleChildren), innerHtml);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/phase-banner/index.js
var require_phase_banner = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/phase-banner/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.PhaseBanner = PhaseBanner;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "tag", "children"];
    function PhaseBanner(props) {
      var className = props.className, tag = props.tag, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-phase-banner ".concat(className || "")
      }, attributes), _react["default"].createElement("p", {
        className: "govuk-phase-banner__content"
      }, _react["default"].createElement(_.Tag, {
        className: "govuk-phase-banner__content__tag ".concat((tag === null || tag === void 0 ? void 0 : tag.className) || "")
      }, tag && tag.children), _react["default"].createElement("span", {
        className: "govuk-phase-banner__text"
      }, children)));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/radios/index.js
var require_radios = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/radios/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Radios = Radios;
    var _extends2 = _interopRequireDefault(require_extends());
    var _defineProperty2 = _interopRequireDefault(require_defineProperty());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _Boolean = require_Boolean();
    var _excluded = ["value", "defaultValue", "items"];
    function ownKeys(object, enumerableOnly) {
      var keys = Object.keys(object);
      if (Object.getOwnPropertySymbols) {
        var symbols = Object.getOwnPropertySymbols(object);
        enumerableOnly && (symbols = symbols.filter(function(sym) {
          return Object.getOwnPropertyDescriptor(object, sym).enumerable;
        })), keys.push.apply(keys, symbols);
      }
      return keys;
    }
    function _objectSpread(target) {
      for (var i = 1; i < arguments.length; i++) {
        var source = null != arguments[i] ? arguments[i] : {};
        i % 2 ? ownKeys(Object(source), true).forEach(function(key) {
          (0, _defineProperty2["default"])(target, key, source[key]);
        }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)) : ownKeys(Object(source)).forEach(function(key) {
          Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));
        });
      }
      return target;
    }
    function Radios(props) {
      var value = props.value, defaultValue = props.defaultValue, items = props.items, restProps = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var processedItems = items ? items.map(function(item) {
        if (item) {
          return _objectSpread(_objectSpread(_objectSpread({}, item), value != null && {
            checked: item.value === value
          }), defaultValue != null && {
            defaultChecked: item.value === defaultValue
          });
        }
        return item;
      }) : null;
      return _react["default"].createElement(_Boolean.Boolean, (0, _extends2["default"])({
        items: processedItems
      }, restProps, {
        controlType: "radios"
      }));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/select/index.js
var require_select = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/select/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Select = void 0;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "aria-describedby", "errorMessage", "formGroup", "hint", "id", "items", "label"];
    var _excluded2 = ["reactListKey", "children"];
    var Select = _react["default"].forwardRef(function(props, ref) {
      var className = props.className, describedBy = props["aria-describedby"], errorMessage = props.errorMessage, formGroup = props.formGroup, hint = props.hint, id = props.id, items = props.items, label = props.label, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var describedByValue = describedBy || "";
      var hintComponent;
      var errorMessageComponent;
      if (hint) {
        var hintId = "".concat(id, "-hint");
        describedByValue += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_.Hint, (0, _extends2["default"])({}, hint, {
          id: hintId
        }));
      }
      if (errorMessage) {
        var errorId = id ? "".concat(id, "-error") : "";
        describedByValue += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      var options = items ? items.filter(function(item) {
        return item;
      }).map(function(option, index) {
        var reactListKey = option.reactListKey, children = option.children, optionAttributes = (0, _objectWithoutProperties2["default"])(option, _excluded2);
        return _react["default"].createElement("option", (0, _extends2["default"])({}, optionAttributes, {
          key: reactListKey || index
        }), children);
      }) : null;
      return _react["default"].createElement("div", {
        className: "govuk-form-group".concat(errorMessage ? " govuk-form-group--error" : "", " ").concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "")
      }, _react["default"].createElement(_.Label, (0, _extends2["default"])({}, label, {
        htmlFor: id
      })), hintComponent, errorMessageComponent, _react["default"].createElement("select", (0, _extends2["default"])({
        className: "govuk-select ".concat(className || "").concat(errorMessage ? " govuk-select--error" : ""),
        id,
        ref,
        "aria-describedby": describedByValue || null
      }, attributes), options));
    });
    exports.Select = Select;
    Select.displayName = "Select";
  }
});

// node_modules/govuk-react-jsx/govuk/components/skip-link/index.js
var require_skip_link = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/skip-link/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.SkipLink = SkipLink;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _excluded = ["href", "className", "children"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function SkipLink(props) {
      var href = props.href, className = props.className, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var skipLinkRef = _react["default"].createRef();
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, SkipLinkJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-skip-link" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./skip-link-TENGIWAG.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  SkipLinkJS = _yield$import["default"];
                  if (skipLinkRef.current) {
                    new SkipLinkJS(skipLinkRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [skipLinkRef]);
      return _react["default"].createElement("a", (0, _extends2["default"])({
        href,
        className: "govuk-skip-link ".concat(className || ""),
        "data-module": "govuk-skip-link",
        ref: skipLinkRef
      }, attributes), children);
    }
    SkipLink.defaultProps = {
      href: "#content"
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/summary-list/index.js
var require_summary_list = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/summary-list/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.SummaryList = SummaryList;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _Link = require_Link();
    var _excluded = ["children", "visuallyHiddenText", "className", "href", "to"];
    var _excluded2 = ["reactListKey"];
    var _excluded3 = ["className", "rows"];
    function ActionLink(props) {
      var children = props.children, visuallyHiddenText = props.visuallyHiddenText, className = props.className, href = props.href, to = props.to, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var contents = _react["default"].createElement(_react["default"].Fragment, null, children, visuallyHiddenText && _react["default"].createElement("span", {
        className: "govuk-visually-hidden"
      }, visuallyHiddenText));
      return _react["default"].createElement(_Link.Link, (0, _extends2["default"])({
        className: "govuk-link ".concat(className || ""),
        to,
        href
      }, attributes), contents);
    }
    function actions(row) {
      var _row$actions, _row$actions2;
      var actionLinks = (_row$actions = row.actions) === null || _row$actions === void 0 ? void 0 : _row$actions.items.map(function(action, index) {
        var reactListKey = action.reactListKey, actionAttributes = (0, _objectWithoutProperties2["default"])(action, _excluded2);
        return _react["default"].createElement(ActionLink, (0, _extends2["default"])({
          key: reactListKey || index
        }, actionAttributes));
      });
      if ((_row$actions2 = row.actions) !== null && _row$actions2 !== void 0 && _row$actions2.items.length) {
        return _react["default"].createElement("dd", {
          className: "govuk-summary-list__actions ".concat(row.actions.className || "")
        }, row.actions.items.length === 1 ? actionLinks : _react["default"].createElement("ul", {
          className: "govuk-summary-list__actions-list"
        }, actionLinks.map(function(actionLink) {
          return _react["default"].createElement("li", {
            key: actionLink.key,
            className: "govuk-summary-list__actions-list-item"
          }, actionLink);
        })));
      }
      return null;
    }
    function SummaryList(props) {
      var className = props.className, rows = props.rows, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded3);
      var filteredRows = rows ? rows.filter(function(row) {
        return row;
      }) : [];
      var anyRowHasActions = filteredRows.some(function(item) {
        var _item$actions;
        return ((_item$actions = item.actions) === null || _item$actions === void 0 ? void 0 : _item$actions.items.length) > 0 === true;
      });
      return _react["default"].createElement("dl", (0, _extends2["default"])({
        className: "govuk-summary-list ".concat(className || "")
      }, attributes), filteredRows.map(function(row, index) {
        var _row$actions3, _row$key, _row$key2, _row$value, _row$value2;
        return _react["default"].createElement("div", {
          key: row.reactListKey || index,
          className: "govuk-summary-list__row ".concat(anyRowHasActions && !((_row$actions3 = row.actions) !== null && _row$actions3 !== void 0 && _row$actions3.items) ? "govuk-summary-list__row--no-actions" : "", " ").concat(row.className || "")
        }, _react["default"].createElement("dt", {
          className: "govuk-summary-list__key ".concat(((_row$key = row.key) === null || _row$key === void 0 ? void 0 : _row$key.className) || "")
        }, (_row$key2 = row.key) === null || _row$key2 === void 0 ? void 0 : _row$key2.children), _react["default"].createElement("dd", {
          className: "govuk-summary-list__value ".concat(((_row$value = row.value) === null || _row$value === void 0 ? void 0 : _row$value.className) || "")
        }, (_row$value2 = row.value) === null || _row$value2 === void 0 ? void 0 : _row$value2.children), actions(row));
      }));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/table/index.js
var require_table = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/table/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Table = Table;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["caption", "captionClassName", "className", "firstCellIsHeader", "head", "rows"];
    var _excluded2 = ["className", "format", "children", "reactListKey"];
    var _excluded3 = ["className", "children", "format", "reactListKey"];
    function Table(props) {
      var caption = props.caption, captionClassName = props.captionClassName, className = props.className, firstCellIsHeader = props.firstCellIsHeader, head = props.head, rows = props.rows, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var captionComponent;
      var headComponent;
      if (caption) {
        captionComponent = _react["default"].createElement("caption", {
          className: "govuk-table__caption ".concat(captionClassName || "")
        }, caption);
      }
      if (head) {
        headComponent = _react["default"].createElement("thead", {
          className: "govuk-table__head"
        }, _react["default"].createElement("tr", {
          className: "govuk-table__row"
        }, head.map(function(item, index) {
          var itemClassName = item.className, itemFormat = item.format, itemChildren = item.children, reactListKey = item.reactListKey, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
          return _react["default"].createElement("th", (0, _extends2["default"])({
            key: reactListKey || index,
            scope: "col",
            className: "govuk-table__header ".concat(itemFormat ? "govuk-table__header--".concat(itemFormat) : "", " ").concat(itemClassName || "")
          }, itemAttributes), itemChildren);
        })));
      }
      var filteredRows = rows ? rows.filter(function(row) {
        return row.cells;
      }) : [];
      return _react["default"].createElement("table", (0, _extends2["default"])({
        className: "govuk-table ".concat(className || "")
      }, attributes), captionComponent, headComponent, _react["default"].createElement("tbody", {
        className: "govuk-table__body"
      }, filteredRows.map(function(row, rowIndex) {
        return _react["default"].createElement("tr", {
          key: row.reactListKey || rowIndex,
          className: "govuk-table__row"
        }, row.cells.map(function(cell, cellIndex) {
          var cellClassName = cell.className, cellChildren = cell.children, cellFormat = cell.format, reactListKey = cell.reactListKey, cellAttributes = (0, _objectWithoutProperties2["default"])(cell, _excluded3);
          if (cellIndex === 0 && firstCellIsHeader) {
            return _react["default"].createElement("th", (0, _extends2["default"])({
              key: reactListKey || cellIndex,
              scope: "row",
              className: "govuk-table__header ".concat(cellClassName || "")
            }, cellAttributes), cellChildren);
          }
          return _react["default"].createElement("td", (0, _extends2["default"])({
            key: cell.reactListKey || cellIndex,
            className: "govuk-table__cell ".concat(cellClassName || "", " ").concat(cellFormat ? "govuk-table__cell--".concat(cellFormat) : "")
          }, cellAttributes), cellChildren);
        }));
      })));
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/tabs/index.js
var require_tabs = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/tabs/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Tabs = Tabs;
    var _regenerator = _interopRequireDefault(require_regenerator());
    var _extends2 = _interopRequireDefault(require_extends());
    var _asyncToGenerator2 = _interopRequireDefault(require_asyncToGenerator());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireWildcard(require_react());
    var _excluded = ["className", "id", "idPrefix", "items", "title"];
    var _excluded2 = ["id", "label", "panel"];
    var _excluded3 = ["id", "panel", "label"];
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function Tabs(props) {
      var className = props.className, id = props.id, idPrefix = props.idPrefix, items = props.items, title = props.title, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var tabsRef = (0, _react.useRef)();
      (0, _react.useEffect)(function() {
        (0, _asyncToGenerator2["default"])(_regenerator["default"].mark(function _callee() {
          var _yield$import, TabsJS;
          return _regenerator["default"].wrap(function _callee$(_context) {
            while (1) {
              switch (_context.prev = _context.next) {
                case 0:
                  if (!(typeof document !== "undefined")) {
                    _context.next = 6;
                    break;
                  }
                  _context.next = 3;
                  return import(
                    /* webpackChunkName: "govuk-frontend-tabs" */
                    /* webpackMode: "lazy" */
                    /* webpackPrefetch: true */
                    "./tabs-HK6F6BRA.js"
                  );
                case 3:
                  _yield$import = _context.sent;
                  TabsJS = _yield$import["default"];
                  if (tabsRef.current) {
                    new TabsJS(tabsRef.current).init();
                  }
                case 6:
                case "end":
                  return _context.stop();
              }
            }
          }, _callee);
        }))();
      }, [tabsRef]);
      var filteredItems = items ? items.filter(function(item) {
        return item;
      }) : [];
      var tabContent = filteredItems.map(function(item, index) {
        var itemId = item.id, label = item.label, panel = item.panel, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded2);
        var tabId = "".concat(itemId || "".concat(idPrefix, "-").concat(index + 1));
        return _react["default"].createElement("li", {
          key: tabId,
          className: "govuk-tabs__list-item".concat(index === 0 ? " govuk-tabs__list-item--selected" : "")
        }, _react["default"].createElement("a", (0, _extends2["default"])({
          className: "govuk-tabs__tab",
          href: "#".concat(tabId)
        }, itemAttributes), label));
      });
      var tabs = filteredItems.length > 0 ? _react["default"].createElement("ul", {
        className: "govuk-tabs__list"
      }, tabContent) : null;
      var panels = filteredItems.map(function(item, index) {
        var itemId = item.id, panel = item.panel, label = item.label, itemAttributes = (0, _objectWithoutProperties2["default"])(item, _excluded3);
        var panelId = "".concat(itemId || "".concat(idPrefix, "-").concat(index + 1));
        return _react["default"].createElement("div", (0, _extends2["default"])({
          key: panelId,
          className: "govuk-tabs__panel".concat(index > 0 ? " govuk-tabs__panel--hidden" : ""),
          id: panelId
        }, panel));
      });
      return _react["default"].createElement("div", (0, _extends2["default"])({
        id,
        className: "govuk-tabs ".concat(className || "")
      }, attributes, {
        "data-module": "govuk-tabs",
        ref: tabsRef
      }), _react["default"].createElement("h2", {
        className: "govuk-tabs__title"
      }, title), tabs, panels);
    }
    Tabs.defaultProps = {
      title: "Contents",
      idPrefix: ""
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/tag/index.js
var require_tag = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/tag/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Tag = Tag;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["children", "className"];
    function Tag(props) {
      var children = props.children, className = props.className, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("strong", (0, _extends2["default"])({
        className: "govuk-tag ".concat(className || "")
      }, attributes), children);
    }
  }
});

// node_modules/govuk-react-jsx/govuk/components/textarea/index.js
var require_textarea = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/textarea/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Textarea = void 0;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _ = require_govuk();
    var _excluded = ["className", "aria-describedby", "errorMessage", "formGroup", "hint", "label", "id"];
    var Textarea = _react["default"].forwardRef(function(props, ref) {
      var className = props.className, describedBy = props["aria-describedby"], errorMessage = props.errorMessage, formGroup = props.formGroup, hint = props.hint, label = props.label, id = props.id, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      var describedByValue = describedBy;
      var hintComponent;
      var errorMessageComponent;
      if (hint) {
        var hintId = "".concat(id, "-hint");
        describedByValue += " ".concat(hintId);
        hintComponent = _react["default"].createElement(_.Hint, (0, _extends2["default"])({}, hint, {
          id: hintId
        }));
      }
      if (errorMessage) {
        var errorId = id ? "".concat(id, "-error") : "";
        describedByValue += " ".concat(errorId);
        errorMessageComponent = _react["default"].createElement(_.ErrorMessage, (0, _extends2["default"])({}, errorMessage, {
          id: errorId
        }));
      }
      return _react["default"].createElement("div", {
        className: "govuk-form-group".concat(errorMessage ? " govuk-form-group--error" : "", " ").concat((formGroup === null || formGroup === void 0 ? void 0 : formGroup.className) || "")
      }, _react["default"].createElement(_.Label, (0, _extends2["default"])({}, label, {
        htmlFor: id
      })), hintComponent, errorMessageComponent, _react["default"].createElement("textarea", (0, _extends2["default"])({}, attributes, {
        id,
        ref,
        className: "govuk-textarea".concat(errorMessage ? " govuk-textarea--error" : "", " ").concat(className || ""),
        "aria-describedby": describedByValue.trim() || null
      })));
    });
    exports.Textarea = Textarea;
    Textarea.displayName = "Textarea";
    Textarea.defaultProps = {
      "aria-describedby": "",
      rows: 5,
      id: "",
      name: ""
    };
  }
});

// node_modules/govuk-react-jsx/govuk/components/warning-text/index.js
var require_warning_text = __commonJS({
  "node_modules/govuk-react-jsx/govuk/components/warning-text/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.WarningText = WarningText;
    var _extends2 = _interopRequireDefault(require_extends());
    var _objectWithoutProperties2 = _interopRequireDefault(require_objectWithoutProperties());
    var _react = _interopRequireDefault(require_react());
    var _excluded = ["className", "iconFallbackText", "children"];
    function WarningText(props) {
      var className = props.className, iconFallbackText = props.iconFallbackText, children = props.children, attributes = (0, _objectWithoutProperties2["default"])(props, _excluded);
      return _react["default"].createElement("div", (0, _extends2["default"])({
        className: "govuk-warning-text ".concat(className || "")
      }, attributes), _react["default"].createElement("span", {
        className: "govuk-warning-text__icon",
        "aria-hidden": "true"
      }, "!"), _react["default"].createElement("strong", {
        className: "govuk-warning-text__text"
      }, _react["default"].createElement("span", {
        className: "govuk-warning-text__assistive"
      }, iconFallbackText), children));
    }
  }
});

// node_modules/react-is/cjs/react-is.development.js
var require_react_is_development = __commonJS({
  "node_modules/react-is/cjs/react-is.development.js"(exports) {
    "use strict";
    if (true) {
      (function() {
        "use strict";
        var hasSymbol = typeof Symbol === "function" && Symbol.for;
        var REACT_ELEMENT_TYPE = hasSymbol ? Symbol.for("react.element") : 60103;
        var REACT_PORTAL_TYPE = hasSymbol ? Symbol.for("react.portal") : 60106;
        var REACT_FRAGMENT_TYPE = hasSymbol ? Symbol.for("react.fragment") : 60107;
        var REACT_STRICT_MODE_TYPE = hasSymbol ? Symbol.for("react.strict_mode") : 60108;
        var REACT_PROFILER_TYPE = hasSymbol ? Symbol.for("react.profiler") : 60114;
        var REACT_PROVIDER_TYPE = hasSymbol ? Symbol.for("react.provider") : 60109;
        var REACT_CONTEXT_TYPE = hasSymbol ? Symbol.for("react.context") : 60110;
        var REACT_ASYNC_MODE_TYPE = hasSymbol ? Symbol.for("react.async_mode") : 60111;
        var REACT_CONCURRENT_MODE_TYPE = hasSymbol ? Symbol.for("react.concurrent_mode") : 60111;
        var REACT_FORWARD_REF_TYPE = hasSymbol ? Symbol.for("react.forward_ref") : 60112;
        var REACT_SUSPENSE_TYPE = hasSymbol ? Symbol.for("react.suspense") : 60113;
        var REACT_SUSPENSE_LIST_TYPE = hasSymbol ? Symbol.for("react.suspense_list") : 60120;
        var REACT_MEMO_TYPE = hasSymbol ? Symbol.for("react.memo") : 60115;
        var REACT_LAZY_TYPE = hasSymbol ? Symbol.for("react.lazy") : 60116;
        var REACT_BLOCK_TYPE = hasSymbol ? Symbol.for("react.block") : 60121;
        var REACT_FUNDAMENTAL_TYPE = hasSymbol ? Symbol.for("react.fundamental") : 60117;
        var REACT_RESPONDER_TYPE = hasSymbol ? Symbol.for("react.responder") : 60118;
        var REACT_SCOPE_TYPE = hasSymbol ? Symbol.for("react.scope") : 60119;
        function isValidElementType(type) {
          return typeof type === "string" || typeof type === "function" || // Note: its typeof might be other than 'symbol' or 'number' if it's a polyfill.
          type === REACT_FRAGMENT_TYPE || type === REACT_CONCURRENT_MODE_TYPE || type === REACT_PROFILER_TYPE || type === REACT_STRICT_MODE_TYPE || type === REACT_SUSPENSE_TYPE || type === REACT_SUSPENSE_LIST_TYPE || typeof type === "object" && type !== null && (type.$$typeof === REACT_LAZY_TYPE || type.$$typeof === REACT_MEMO_TYPE || type.$$typeof === REACT_PROVIDER_TYPE || type.$$typeof === REACT_CONTEXT_TYPE || type.$$typeof === REACT_FORWARD_REF_TYPE || type.$$typeof === REACT_FUNDAMENTAL_TYPE || type.$$typeof === REACT_RESPONDER_TYPE || type.$$typeof === REACT_SCOPE_TYPE || type.$$typeof === REACT_BLOCK_TYPE);
        }
        function typeOf(object) {
          if (typeof object === "object" && object !== null) {
            var $$typeof = object.$$typeof;
            switch ($$typeof) {
              case REACT_ELEMENT_TYPE:
                var type = object.type;
                switch (type) {
                  case REACT_ASYNC_MODE_TYPE:
                  case REACT_CONCURRENT_MODE_TYPE:
                  case REACT_FRAGMENT_TYPE:
                  case REACT_PROFILER_TYPE:
                  case REACT_STRICT_MODE_TYPE:
                  case REACT_SUSPENSE_TYPE:
                    return type;
                  default:
                    var $$typeofType = type && type.$$typeof;
                    switch ($$typeofType) {
                      case REACT_CONTEXT_TYPE:
                      case REACT_FORWARD_REF_TYPE:
                      case REACT_LAZY_TYPE:
                      case REACT_MEMO_TYPE:
                      case REACT_PROVIDER_TYPE:
                        return $$typeofType;
                      default:
                        return $$typeof;
                    }
                }
              case REACT_PORTAL_TYPE:
                return $$typeof;
            }
          }
          return void 0;
        }
        var AsyncMode = REACT_ASYNC_MODE_TYPE;
        var ConcurrentMode = REACT_CONCURRENT_MODE_TYPE;
        var ContextConsumer = REACT_CONTEXT_TYPE;
        var ContextProvider = REACT_PROVIDER_TYPE;
        var Element2 = REACT_ELEMENT_TYPE;
        var ForwardRef = REACT_FORWARD_REF_TYPE;
        var Fragment = REACT_FRAGMENT_TYPE;
        var Lazy = REACT_LAZY_TYPE;
        var Memo = REACT_MEMO_TYPE;
        var Portal = REACT_PORTAL_TYPE;
        var Profiler = REACT_PROFILER_TYPE;
        var StrictMode = REACT_STRICT_MODE_TYPE;
        var Suspense = REACT_SUSPENSE_TYPE;
        var hasWarnedAboutDeprecatedIsAsyncMode = false;
        function isAsyncMode(object) {
          {
            if (!hasWarnedAboutDeprecatedIsAsyncMode) {
              hasWarnedAboutDeprecatedIsAsyncMode = true;
              console["warn"]("The ReactIs.isAsyncMode() alias has been deprecated, and will be removed in React 17+. Update your code to use ReactIs.isConcurrentMode() instead. It has the exact same API.");
            }
          }
          return isConcurrentMode(object) || typeOf(object) === REACT_ASYNC_MODE_TYPE;
        }
        function isConcurrentMode(object) {
          return typeOf(object) === REACT_CONCURRENT_MODE_TYPE;
        }
        function isContextConsumer(object) {
          return typeOf(object) === REACT_CONTEXT_TYPE;
        }
        function isContextProvider(object) {
          return typeOf(object) === REACT_PROVIDER_TYPE;
        }
        function isElement(object) {
          return typeof object === "object" && object !== null && object.$$typeof === REACT_ELEMENT_TYPE;
        }
        function isForwardRef(object) {
          return typeOf(object) === REACT_FORWARD_REF_TYPE;
        }
        function isFragment(object) {
          return typeOf(object) === REACT_FRAGMENT_TYPE;
        }
        function isLazy(object) {
          return typeOf(object) === REACT_LAZY_TYPE;
        }
        function isMemo(object) {
          return typeOf(object) === REACT_MEMO_TYPE;
        }
        function isPortal(object) {
          return typeOf(object) === REACT_PORTAL_TYPE;
        }
        function isProfiler(object) {
          return typeOf(object) === REACT_PROFILER_TYPE;
        }
        function isStrictMode(object) {
          return typeOf(object) === REACT_STRICT_MODE_TYPE;
        }
        function isSuspense(object) {
          return typeOf(object) === REACT_SUSPENSE_TYPE;
        }
        exports.AsyncMode = AsyncMode;
        exports.ConcurrentMode = ConcurrentMode;
        exports.ContextConsumer = ContextConsumer;
        exports.ContextProvider = ContextProvider;
        exports.Element = Element2;
        exports.ForwardRef = ForwardRef;
        exports.Fragment = Fragment;
        exports.Lazy = Lazy;
        exports.Memo = Memo;
        exports.Portal = Portal;
        exports.Profiler = Profiler;
        exports.StrictMode = StrictMode;
        exports.Suspense = Suspense;
        exports.isAsyncMode = isAsyncMode;
        exports.isConcurrentMode = isConcurrentMode;
        exports.isContextConsumer = isContextConsumer;
        exports.isContextProvider = isContextProvider;
        exports.isElement = isElement;
        exports.isForwardRef = isForwardRef;
        exports.isFragment = isFragment;
        exports.isLazy = isLazy;
        exports.isMemo = isMemo;
        exports.isPortal = isPortal;
        exports.isProfiler = isProfiler;
        exports.isStrictMode = isStrictMode;
        exports.isSuspense = isSuspense;
        exports.isValidElementType = isValidElementType;
        exports.typeOf = typeOf;
      })();
    }
  }
});

// node_modules/react-is/index.js
var require_react_is = __commonJS({
  "node_modules/react-is/index.js"(exports, module) {
    "use strict";
    if (false) {
      module.exports = null;
    } else {
      module.exports = require_react_is_development();
    }
  }
});

// node_modules/object-assign/index.js
var require_object_assign = __commonJS({
  "node_modules/object-assign/index.js"(exports, module) {
    "use strict";
    var getOwnPropertySymbols = Object.getOwnPropertySymbols;
    var hasOwnProperty = Object.prototype.hasOwnProperty;
    var propIsEnumerable = Object.prototype.propertyIsEnumerable;
    function toObject(val) {
      if (val === null || val === void 0) {
        throw new TypeError("Object.assign cannot be called with null or undefined");
      }
      return Object(val);
    }
    function shouldUseNative() {
      try {
        if (!Object.assign) {
          return false;
        }
        var test1 = new String("abc");
        test1[5] = "de";
        if (Object.getOwnPropertyNames(test1)[0] === "5") {
          return false;
        }
        var test2 = {};
        for (var i = 0; i < 10; i++) {
          test2["_" + String.fromCharCode(i)] = i;
        }
        var order2 = Object.getOwnPropertyNames(test2).map(function(n) {
          return test2[n];
        });
        if (order2.join("") !== "0123456789") {
          return false;
        }
        var test3 = {};
        "abcdefghijklmnopqrst".split("").forEach(function(letter) {
          test3[letter] = letter;
        });
        if (Object.keys(Object.assign({}, test3)).join("") !== "abcdefghijklmnopqrst") {
          return false;
        }
        return true;
      } catch (err) {
        return false;
      }
    }
    module.exports = shouldUseNative() ? Object.assign : function(target, source) {
      var from;
      var to = toObject(target);
      var symbols;
      for (var s = 1; s < arguments.length; s++) {
        from = Object(arguments[s]);
        for (var key in from) {
          if (hasOwnProperty.call(from, key)) {
            to[key] = from[key];
          }
        }
        if (getOwnPropertySymbols) {
          symbols = getOwnPropertySymbols(from);
          for (var i = 0; i < symbols.length; i++) {
            if (propIsEnumerable.call(from, symbols[i])) {
              to[symbols[i]] = from[symbols[i]];
            }
          }
        }
      }
      return to;
    };
  }
});

// node_modules/prop-types/lib/ReactPropTypesSecret.js
var require_ReactPropTypesSecret = __commonJS({
  "node_modules/prop-types/lib/ReactPropTypesSecret.js"(exports, module) {
    "use strict";
    var ReactPropTypesSecret = "SECRET_DO_NOT_PASS_THIS_OR_YOU_WILL_BE_FIRED";
    module.exports = ReactPropTypesSecret;
  }
});

// node_modules/prop-types/lib/has.js
var require_has = __commonJS({
  "node_modules/prop-types/lib/has.js"(exports, module) {
    module.exports = Function.call.bind(Object.prototype.hasOwnProperty);
  }
});

// node_modules/prop-types/checkPropTypes.js
var require_checkPropTypes = __commonJS({
  "node_modules/prop-types/checkPropTypes.js"(exports, module) {
    "use strict";
    var printWarning = function() {
    };
    if (true) {
      ReactPropTypesSecret = require_ReactPropTypesSecret();
      loggedTypeFailures = {};
      has = require_has();
      printWarning = function(text) {
        var message = "Warning: " + text;
        if (typeof console !== "undefined") {
          console.error(message);
        }
        try {
          throw new Error(message);
        } catch (x) {
        }
      };
    }
    var ReactPropTypesSecret;
    var loggedTypeFailures;
    var has;
    function checkPropTypes(typeSpecs, values, location, componentName, getStack) {
      if (true) {
        for (var typeSpecName in typeSpecs) {
          if (has(typeSpecs, typeSpecName)) {
            var error;
            try {
              if (typeof typeSpecs[typeSpecName] !== "function") {
                var err = Error(
                  (componentName || "React class") + ": " + location + " type `" + typeSpecName + "` is invalid; it must be a function, usually from the `prop-types` package, but received `" + typeof typeSpecs[typeSpecName] + "`.This often happens because of typos such as `PropTypes.function` instead of `PropTypes.func`."
                );
                err.name = "Invariant Violation";
                throw err;
              }
              error = typeSpecs[typeSpecName](values, typeSpecName, componentName, location, null, ReactPropTypesSecret);
            } catch (ex) {
              error = ex;
            }
            if (error && !(error instanceof Error)) {
              printWarning(
                (componentName || "React class") + ": type specification of " + location + " `" + typeSpecName + "` is invalid; the type checker function must return `null` or an `Error` but returned a " + typeof error + ". You may have forgotten to pass an argument to the type checker creator (arrayOf, instanceOf, objectOf, oneOf, oneOfType, and shape all require an argument)."
              );
            }
            if (error instanceof Error && !(error.message in loggedTypeFailures)) {
              loggedTypeFailures[error.message] = true;
              var stack = getStack ? getStack() : "";
              printWarning(
                "Failed " + location + " type: " + error.message + (stack != null ? stack : "")
              );
            }
          }
        }
      }
    }
    checkPropTypes.resetWarningCache = function() {
      if (true) {
        loggedTypeFailures = {};
      }
    };
    module.exports = checkPropTypes;
  }
});

// node_modules/prop-types/factoryWithTypeCheckers.js
var require_factoryWithTypeCheckers = __commonJS({
  "node_modules/prop-types/factoryWithTypeCheckers.js"(exports, module) {
    "use strict";
    var ReactIs = require_react_is();
    var assign = require_object_assign();
    var ReactPropTypesSecret = require_ReactPropTypesSecret();
    var has = require_has();
    var checkPropTypes = require_checkPropTypes();
    var printWarning = function() {
    };
    if (true) {
      printWarning = function(text) {
        var message = "Warning: " + text;
        if (typeof console !== "undefined") {
          console.error(message);
        }
        try {
          throw new Error(message);
        } catch (x) {
        }
      };
    }
    function emptyFunctionThatReturnsNull() {
      return null;
    }
    module.exports = function(isValidElement, throwOnDirectAccess) {
      var ITERATOR_SYMBOL = typeof Symbol === "function" && Symbol.iterator;
      var FAUX_ITERATOR_SYMBOL = "@@iterator";
      function getIteratorFn(maybeIterable) {
        var iteratorFn = maybeIterable && (ITERATOR_SYMBOL && maybeIterable[ITERATOR_SYMBOL] || maybeIterable[FAUX_ITERATOR_SYMBOL]);
        if (typeof iteratorFn === "function") {
          return iteratorFn;
        }
      }
      var ANONYMOUS = "<<anonymous>>";
      var ReactPropTypes = {
        array: createPrimitiveTypeChecker("array"),
        bigint: createPrimitiveTypeChecker("bigint"),
        bool: createPrimitiveTypeChecker("boolean"),
        func: createPrimitiveTypeChecker("function"),
        number: createPrimitiveTypeChecker("number"),
        object: createPrimitiveTypeChecker("object"),
        string: createPrimitiveTypeChecker("string"),
        symbol: createPrimitiveTypeChecker("symbol"),
        any: createAnyTypeChecker(),
        arrayOf: createArrayOfTypeChecker,
        element: createElementTypeChecker(),
        elementType: createElementTypeTypeChecker(),
        instanceOf: createInstanceTypeChecker,
        node: createNodeChecker(),
        objectOf: createObjectOfTypeChecker,
        oneOf: createEnumTypeChecker,
        oneOfType: createUnionTypeChecker,
        shape: createShapeTypeChecker,
        exact: createStrictShapeTypeChecker
      };
      function is(x, y) {
        if (x === y) {
          return x !== 0 || 1 / x === 1 / y;
        } else {
          return x !== x && y !== y;
        }
      }
      function PropTypeError(message, data) {
        this.message = message;
        this.data = data && typeof data === "object" ? data : {};
        this.stack = "";
      }
      PropTypeError.prototype = Error.prototype;
      function createChainableTypeChecker(validate) {
        if (true) {
          var manualPropTypeCallCache = {};
          var manualPropTypeWarningCount = 0;
        }
        function checkType(isRequired, props, propName, componentName, location, propFullName, secret) {
          componentName = componentName || ANONYMOUS;
          propFullName = propFullName || propName;
          if (secret !== ReactPropTypesSecret) {
            if (throwOnDirectAccess) {
              var err = new Error(
                "Calling PropTypes validators directly is not supported by the `prop-types` package. Use `PropTypes.checkPropTypes()` to call them. Read more at http://fb.me/use-check-prop-types"
              );
              err.name = "Invariant Violation";
              throw err;
            } else if (typeof console !== "undefined") {
              var cacheKey = componentName + ":" + propName;
              if (!manualPropTypeCallCache[cacheKey] && // Avoid spamming the console because they are often not actionable except for lib authors
              manualPropTypeWarningCount < 3) {
                printWarning(
                  "You are manually calling a React.PropTypes validation function for the `" + propFullName + "` prop on `" + componentName + "`. This is deprecated and will throw in the standalone `prop-types` package. You may be seeing this warning due to a third-party PropTypes library. See https://fb.me/react-warning-dont-call-proptypes for details."
                );
                manualPropTypeCallCache[cacheKey] = true;
                manualPropTypeWarningCount++;
              }
            }
          }
          if (props[propName] == null) {
            if (isRequired) {
              if (props[propName] === null) {
                return new PropTypeError("The " + location + " `" + propFullName + "` is marked as required " + ("in `" + componentName + "`, but its value is `null`."));
              }
              return new PropTypeError("The " + location + " `" + propFullName + "` is marked as required in " + ("`" + componentName + "`, but its value is `undefined`."));
            }
            return null;
          } else {
            return validate(props, propName, componentName, location, propFullName);
          }
        }
        var chainedCheckType = checkType.bind(null, false);
        chainedCheckType.isRequired = checkType.bind(null, true);
        return chainedCheckType;
      }
      function createPrimitiveTypeChecker(expectedType) {
        function validate(props, propName, componentName, location, propFullName, secret) {
          var propValue = props[propName];
          var propType = getPropType(propValue);
          if (propType !== expectedType) {
            var preciseType = getPreciseType(propValue);
            return new PropTypeError(
              "Invalid " + location + " `" + propFullName + "` of type " + ("`" + preciseType + "` supplied to `" + componentName + "`, expected ") + ("`" + expectedType + "`."),
              { expectedType }
            );
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createAnyTypeChecker() {
        return createChainableTypeChecker(emptyFunctionThatReturnsNull);
      }
      function createArrayOfTypeChecker(typeChecker) {
        function validate(props, propName, componentName, location, propFullName) {
          if (typeof typeChecker !== "function") {
            return new PropTypeError("Property `" + propFullName + "` of component `" + componentName + "` has invalid PropType notation inside arrayOf.");
          }
          var propValue = props[propName];
          if (!Array.isArray(propValue)) {
            var propType = getPropType(propValue);
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type " + ("`" + propType + "` supplied to `" + componentName + "`, expected an array."));
          }
          for (var i = 0; i < propValue.length; i++) {
            var error = typeChecker(propValue, i, componentName, location, propFullName + "[" + i + "]", ReactPropTypesSecret);
            if (error instanceof Error) {
              return error;
            }
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createElementTypeChecker() {
        function validate(props, propName, componentName, location, propFullName) {
          var propValue = props[propName];
          if (!isValidElement(propValue)) {
            var propType = getPropType(propValue);
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type " + ("`" + propType + "` supplied to `" + componentName + "`, expected a single ReactElement."));
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createElementTypeTypeChecker() {
        function validate(props, propName, componentName, location, propFullName) {
          var propValue = props[propName];
          if (!ReactIs.isValidElementType(propValue)) {
            var propType = getPropType(propValue);
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type " + ("`" + propType + "` supplied to `" + componentName + "`, expected a single ReactElement type."));
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createInstanceTypeChecker(expectedClass) {
        function validate(props, propName, componentName, location, propFullName) {
          if (!(props[propName] instanceof expectedClass)) {
            var expectedClassName = expectedClass.name || ANONYMOUS;
            var actualClassName = getClassName(props[propName]);
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type " + ("`" + actualClassName + "` supplied to `" + componentName + "`, expected ") + ("instance of `" + expectedClassName + "`."));
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createEnumTypeChecker(expectedValues) {
        if (!Array.isArray(expectedValues)) {
          if (true) {
            if (arguments.length > 1) {
              printWarning(
                "Invalid arguments supplied to oneOf, expected an array, got " + arguments.length + " arguments. A common mistake is to write oneOf(x, y, z) instead of oneOf([x, y, z])."
              );
            } else {
              printWarning("Invalid argument supplied to oneOf, expected an array.");
            }
          }
          return emptyFunctionThatReturnsNull;
        }
        function validate(props, propName, componentName, location, propFullName) {
          var propValue = props[propName];
          for (var i = 0; i < expectedValues.length; i++) {
            if (is(propValue, expectedValues[i])) {
              return null;
            }
          }
          var valuesString = JSON.stringify(expectedValues, function replacer(key, value) {
            var type = getPreciseType(value);
            if (type === "symbol") {
              return String(value);
            }
            return value;
          });
          return new PropTypeError("Invalid " + location + " `" + propFullName + "` of value `" + String(propValue) + "` " + ("supplied to `" + componentName + "`, expected one of " + valuesString + "."));
        }
        return createChainableTypeChecker(validate);
      }
      function createObjectOfTypeChecker(typeChecker) {
        function validate(props, propName, componentName, location, propFullName) {
          if (typeof typeChecker !== "function") {
            return new PropTypeError("Property `" + propFullName + "` of component `" + componentName + "` has invalid PropType notation inside objectOf.");
          }
          var propValue = props[propName];
          var propType = getPropType(propValue);
          if (propType !== "object") {
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type " + ("`" + propType + "` supplied to `" + componentName + "`, expected an object."));
          }
          for (var key in propValue) {
            if (has(propValue, key)) {
              var error = typeChecker(propValue, key, componentName, location, propFullName + "." + key, ReactPropTypesSecret);
              if (error instanceof Error) {
                return error;
              }
            }
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createUnionTypeChecker(arrayOfTypeCheckers) {
        if (!Array.isArray(arrayOfTypeCheckers)) {
          true ? printWarning("Invalid argument supplied to oneOfType, expected an instance of array.") : void 0;
          return emptyFunctionThatReturnsNull;
        }
        for (var i = 0; i < arrayOfTypeCheckers.length; i++) {
          var checker = arrayOfTypeCheckers[i];
          if (typeof checker !== "function") {
            printWarning(
              "Invalid argument supplied to oneOfType. Expected an array of check functions, but received " + getPostfixForTypeWarning(checker) + " at index " + i + "."
            );
            return emptyFunctionThatReturnsNull;
          }
        }
        function validate(props, propName, componentName, location, propFullName) {
          var expectedTypes = [];
          for (var i2 = 0; i2 < arrayOfTypeCheckers.length; i2++) {
            var checker2 = arrayOfTypeCheckers[i2];
            var checkerResult = checker2(props, propName, componentName, location, propFullName, ReactPropTypesSecret);
            if (checkerResult == null) {
              return null;
            }
            if (checkerResult.data && has(checkerResult.data, "expectedType")) {
              expectedTypes.push(checkerResult.data.expectedType);
            }
          }
          var expectedTypesMessage = expectedTypes.length > 0 ? ", expected one of type [" + expectedTypes.join(", ") + "]" : "";
          return new PropTypeError("Invalid " + location + " `" + propFullName + "` supplied to " + ("`" + componentName + "`" + expectedTypesMessage + "."));
        }
        return createChainableTypeChecker(validate);
      }
      function createNodeChecker() {
        function validate(props, propName, componentName, location, propFullName) {
          if (!isNode(props[propName])) {
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` supplied to " + ("`" + componentName + "`, expected a ReactNode."));
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function invalidValidatorError(componentName, location, propFullName, key, type) {
        return new PropTypeError(
          (componentName || "React class") + ": " + location + " type `" + propFullName + "." + key + "` is invalid; it must be a function, usually from the `prop-types` package, but received `" + type + "`."
        );
      }
      function createShapeTypeChecker(shapeTypes) {
        function validate(props, propName, componentName, location, propFullName) {
          var propValue = props[propName];
          var propType = getPropType(propValue);
          if (propType !== "object") {
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type `" + propType + "` " + ("supplied to `" + componentName + "`, expected `object`."));
          }
          for (var key in shapeTypes) {
            var checker = shapeTypes[key];
            if (typeof checker !== "function") {
              return invalidValidatorError(componentName, location, propFullName, key, getPreciseType(checker));
            }
            var error = checker(propValue, key, componentName, location, propFullName + "." + key, ReactPropTypesSecret);
            if (error) {
              return error;
            }
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function createStrictShapeTypeChecker(shapeTypes) {
        function validate(props, propName, componentName, location, propFullName) {
          var propValue = props[propName];
          var propType = getPropType(propValue);
          if (propType !== "object") {
            return new PropTypeError("Invalid " + location + " `" + propFullName + "` of type `" + propType + "` " + ("supplied to `" + componentName + "`, expected `object`."));
          }
          var allKeys = assign({}, props[propName], shapeTypes);
          for (var key in allKeys) {
            var checker = shapeTypes[key];
            if (has(shapeTypes, key) && typeof checker !== "function") {
              return invalidValidatorError(componentName, location, propFullName, key, getPreciseType(checker));
            }
            if (!checker) {
              return new PropTypeError(
                "Invalid " + location + " `" + propFullName + "` key `" + key + "` supplied to `" + componentName + "`.\nBad object: " + JSON.stringify(props[propName], null, "  ") + "\nValid keys: " + JSON.stringify(Object.keys(shapeTypes), null, "  ")
              );
            }
            var error = checker(propValue, key, componentName, location, propFullName + "." + key, ReactPropTypesSecret);
            if (error) {
              return error;
            }
          }
          return null;
        }
        return createChainableTypeChecker(validate);
      }
      function isNode(propValue) {
        switch (typeof propValue) {
          case "number":
          case "string":
          case "undefined":
            return true;
          case "boolean":
            return !propValue;
          case "object":
            if (Array.isArray(propValue)) {
              return propValue.every(isNode);
            }
            if (propValue === null || isValidElement(propValue)) {
              return true;
            }
            var iteratorFn = getIteratorFn(propValue);
            if (iteratorFn) {
              var iterator = iteratorFn.call(propValue);
              var step;
              if (iteratorFn !== propValue.entries) {
                while (!(step = iterator.next()).done) {
                  if (!isNode(step.value)) {
                    return false;
                  }
                }
              } else {
                while (!(step = iterator.next()).done) {
                  var entry = step.value;
                  if (entry) {
                    if (!isNode(entry[1])) {
                      return false;
                    }
                  }
                }
              }
            } else {
              return false;
            }
            return true;
          default:
            return false;
        }
      }
      function isSymbol(propType, propValue) {
        if (propType === "symbol") {
          return true;
        }
        if (!propValue) {
          return false;
        }
        if (propValue["@@toStringTag"] === "Symbol") {
          return true;
        }
        if (typeof Symbol === "function" && propValue instanceof Symbol) {
          return true;
        }
        return false;
      }
      function getPropType(propValue) {
        var propType = typeof propValue;
        if (Array.isArray(propValue)) {
          return "array";
        }
        if (propValue instanceof RegExp) {
          return "object";
        }
        if (isSymbol(propType, propValue)) {
          return "symbol";
        }
        return propType;
      }
      function getPreciseType(propValue) {
        if (typeof propValue === "undefined" || propValue === null) {
          return "" + propValue;
        }
        var propType = getPropType(propValue);
        if (propType === "object") {
          if (propValue instanceof Date) {
            return "date";
          } else if (propValue instanceof RegExp) {
            return "regexp";
          }
        }
        return propType;
      }
      function getPostfixForTypeWarning(value) {
        var type = getPreciseType(value);
        switch (type) {
          case "array":
          case "object":
            return "an " + type;
          case "boolean":
          case "date":
          case "regexp":
            return "a " + type;
          default:
            return type;
        }
      }
      function getClassName(propValue) {
        if (!propValue.constructor || !propValue.constructor.name) {
          return ANONYMOUS;
        }
        return propValue.constructor.name;
      }
      ReactPropTypes.checkPropTypes = checkPropTypes;
      ReactPropTypes.resetWarningCache = checkPropTypes.resetWarningCache;
      ReactPropTypes.PropTypes = ReactPropTypes;
      return ReactPropTypes;
    };
  }
});

// node_modules/prop-types/index.js
var require_prop_types = __commonJS({
  "node_modules/prop-types/index.js"(exports, module) {
    if (true) {
      ReactIs = require_react_is();
      throwOnDirectAccess = true;
      module.exports = require_factoryWithTypeCheckers()(ReactIs.isElement, throwOnDirectAccess);
    } else {
      module.exports = null();
    }
    var ReactIs;
    var throwOnDirectAccess;
  }
});

// node_modules/react-helmet/node_modules/react-side-effect/lib/index.js
var require_lib = __commonJS({
  "node_modules/react-helmet/node_modules/react-side-effect/lib/index.js"(exports, module) {
    "use strict";
    function _interopDefault(ex) {
      return ex && typeof ex === "object" && "default" in ex ? ex["default"] : ex;
    }
    var React2 = require_react();
    var React__default = _interopDefault(React2);
    function _defineProperty(obj, key, value) {
      if (key in obj) {
        Object.defineProperty(obj, key, {
          value,
          enumerable: true,
          configurable: true,
          writable: true
        });
      } else {
        obj[key] = value;
      }
      return obj;
    }
    function _inheritsLoose(subClass, superClass) {
      subClass.prototype = Object.create(superClass.prototype);
      subClass.prototype.constructor = subClass;
      subClass.__proto__ = superClass;
    }
    var canUseDOM = !!(typeof window !== "undefined" && window.document && window.document.createElement);
    function withSideEffect2(reducePropsToState3, handleStateChangeOnClient, mapStateOnServer3) {
      if (typeof reducePropsToState3 !== "function") {
        throw new Error("Expected reducePropsToState to be a function.");
      }
      if (typeof handleStateChangeOnClient !== "function") {
        throw new Error("Expected handleStateChangeOnClient to be a function.");
      }
      if (typeof mapStateOnServer3 !== "undefined" && typeof mapStateOnServer3 !== "function") {
        throw new Error("Expected mapStateOnServer to either be undefined or a function.");
      }
      function getDisplayName(WrappedComponent) {
        return WrappedComponent.displayName || WrappedComponent.name || "Component";
      }
      return function wrap(WrappedComponent) {
        if (typeof WrappedComponent !== "function") {
          throw new Error("Expected WrappedComponent to be a React component.");
        }
        var mountedInstances = [];
        var state;
        function emitChange() {
          state = reducePropsToState3(mountedInstances.map(function(instance) {
            return instance.props;
          }));
          if (SideEffect.canUseDOM) {
            handleStateChangeOnClient(state);
          } else if (mapStateOnServer3) {
            state = mapStateOnServer3(state);
          }
        }
        var SideEffect = function(_PureComponent) {
          _inheritsLoose(SideEffect2, _PureComponent);
          function SideEffect2() {
            return _PureComponent.apply(this, arguments) || this;
          }
          SideEffect2.peek = function peek() {
            return state;
          };
          SideEffect2.rewind = function rewind() {
            if (SideEffect2.canUseDOM) {
              throw new Error("You may only call rewind() on the server. Call peek() to read the current state.");
            }
            var recordedState = state;
            state = void 0;
            mountedInstances = [];
            return recordedState;
          };
          var _proto = SideEffect2.prototype;
          _proto.UNSAFE_componentWillMount = function UNSAFE_componentWillMount() {
            mountedInstances.push(this);
            emitChange();
          };
          _proto.componentDidUpdate = function componentDidUpdate() {
            emitChange();
          };
          _proto.componentWillUnmount = function componentWillUnmount() {
            var index = mountedInstances.indexOf(this);
            mountedInstances.splice(index, 1);
            emitChange();
          };
          _proto.render = function render() {
            return React__default.createElement(WrappedComponent, this.props);
          };
          return SideEffect2;
        }(React2.PureComponent);
        _defineProperty(SideEffect, "displayName", "SideEffect(" + getDisplayName(WrappedComponent) + ")");
        _defineProperty(SideEffect, "canUseDOM", canUseDOM);
        return SideEffect;
      };
    }
    module.exports = withSideEffect2;
  }
});

// node_modules/react-fast-compare/index.js
var require_react_fast_compare = __commonJS({
  "node_modules/react-fast-compare/index.js"(exports, module) {
    var hasElementType = typeof Element !== "undefined";
    var hasMap = typeof Map === "function";
    var hasSet = typeof Set === "function";
    var hasArrayBuffer = typeof ArrayBuffer === "function" && !!ArrayBuffer.isView;
    function equal(a, b) {
      if (a === b) return true;
      if (a && b && typeof a == "object" && typeof b == "object") {
        if (a.constructor !== b.constructor) return false;
        var length, i, keys;
        if (Array.isArray(a)) {
          length = a.length;
          if (length != b.length) return false;
          for (i = length; i-- !== 0; )
            if (!equal(a[i], b[i])) return false;
          return true;
        }
        var it;
        if (hasMap && a instanceof Map && b instanceof Map) {
          if (a.size !== b.size) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!b.has(i.value[0])) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!equal(i.value[1], b.get(i.value[0]))) return false;
          return true;
        }
        if (hasSet && a instanceof Set && b instanceof Set) {
          if (a.size !== b.size) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!b.has(i.value[0])) return false;
          return true;
        }
        if (hasArrayBuffer && ArrayBuffer.isView(a) && ArrayBuffer.isView(b)) {
          length = a.length;
          if (length != b.length) return false;
          for (i = length; i-- !== 0; )
            if (a[i] !== b[i]) return false;
          return true;
        }
        if (a.constructor === RegExp) return a.source === b.source && a.flags === b.flags;
        if (a.valueOf !== Object.prototype.valueOf && typeof a.valueOf === "function" && typeof b.valueOf === "function") return a.valueOf() === b.valueOf();
        if (a.toString !== Object.prototype.toString && typeof a.toString === "function" && typeof b.toString === "function") return a.toString() === b.toString();
        keys = Object.keys(a);
        length = keys.length;
        if (length !== Object.keys(b).length) return false;
        for (i = length; i-- !== 0; )
          if (!Object.prototype.hasOwnProperty.call(b, keys[i])) return false;
        if (hasElementType && a instanceof Element) return false;
        for (i = length; i-- !== 0; ) {
          if ((keys[i] === "_owner" || keys[i] === "__v" || keys[i] === "__o") && a.$$typeof) {
            continue;
          }
          if (!equal(a[keys[i]], b[keys[i]])) return false;
        }
        return true;
      }
      return a !== a && b !== b;
    }
    module.exports = function isEqual2(a, b) {
      try {
        return equal(a, b);
      } catch (error) {
        if ((error.message || "").match(/stack|recursion/i)) {
          console.warn("react-fast-compare cannot handle circular refs");
          return false;
        }
        throw error;
      }
    };
  }
});

// node_modules/react-helmet/es/Helmet.js
var Helmet_exports = {};
__export(Helmet_exports, {
  Helmet: () => HelmetExport,
  default: () => Helmet_default
});
var import_prop_types, import_react_side_effect, import_react_fast_compare, import_react, import_object_assign, ATTRIBUTE_NAMES, TAG_NAMES, VALID_TAG_NAMES, TAG_PROPERTIES, REACT_TAG_MAP, HELMET_PROPS, HTML_TAG_MAP, SELF_CLOSING_TAGS, HELMET_ATTRIBUTE, _typeof, classCallCheck, createClass, _extends, inherits, objectWithoutProperties, possibleConstructorReturn, encodeSpecialCharacters, getTitleFromPropsList, getOnChangeClientState, getAttributesFromPropsList, getBaseTagFromPropsList, getTagsFromPropsList, getInnermostProperty, reducePropsToState, rafPolyfill, cafPolyfill, requestAnimationFrame, cancelAnimationFrame, warn, _helmetCallback, handleClientStateChange, commitTagChanges, flattenArray, updateTitle, updateAttributes, updateTags, generateElementAttributesAsString, generateTitleAsString, generateTagsAsString, convertElementAttributestoReactProps, convertReactPropstoHtmlAttributes, generateTitleAsReactComponent, generateTagsAsReactComponent, getMethodsForTag, mapStateOnServer, Helmet, NullComponent, HelmetSideEffects, HelmetExport, Helmet_default;
var init_Helmet = __esm({
  "node_modules/react-helmet/es/Helmet.js"() {
    import_prop_types = __toESM(require_prop_types());
    import_react_side_effect = __toESM(require_lib());
    import_react_fast_compare = __toESM(require_react_fast_compare());
    import_react = __toESM(require_react());
    import_object_assign = __toESM(require_object_assign());
    ATTRIBUTE_NAMES = {
      BODY: "bodyAttributes",
      HTML: "htmlAttributes",
      TITLE: "titleAttributes"
    };
    TAG_NAMES = {
      BASE: "base",
      BODY: "body",
      HEAD: "head",
      HTML: "html",
      LINK: "link",
      META: "meta",
      NOSCRIPT: "noscript",
      SCRIPT: "script",
      STYLE: "style",
      TITLE: "title"
    };
    VALID_TAG_NAMES = Object.keys(TAG_NAMES).map(function(name) {
      return TAG_NAMES[name];
    });
    TAG_PROPERTIES = {
      CHARSET: "charset",
      CSS_TEXT: "cssText",
      HREF: "href",
      HTTPEQUIV: "http-equiv",
      INNER_HTML: "innerHTML",
      ITEM_PROP: "itemprop",
      NAME: "name",
      PROPERTY: "property",
      REL: "rel",
      SRC: "src",
      TARGET: "target"
    };
    REACT_TAG_MAP = {
      accesskey: "accessKey",
      charset: "charSet",
      class: "className",
      contenteditable: "contentEditable",
      contextmenu: "contextMenu",
      "http-equiv": "httpEquiv",
      itemprop: "itemProp",
      tabindex: "tabIndex"
    };
    HELMET_PROPS = {
      DEFAULT_TITLE: "defaultTitle",
      DEFER: "defer",
      ENCODE_SPECIAL_CHARACTERS: "encodeSpecialCharacters",
      ON_CHANGE_CLIENT_STATE: "onChangeClientState",
      TITLE_TEMPLATE: "titleTemplate"
    };
    HTML_TAG_MAP = Object.keys(REACT_TAG_MAP).reduce(function(obj, key) {
      obj[REACT_TAG_MAP[key]] = key;
      return obj;
    }, {});
    SELF_CLOSING_TAGS = [TAG_NAMES.NOSCRIPT, TAG_NAMES.SCRIPT, TAG_NAMES.STYLE];
    HELMET_ATTRIBUTE = "data-react-helmet";
    _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function(obj) {
      return typeof obj;
    } : function(obj) {
      return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj;
    };
    classCallCheck = function(instance, Constructor) {
      if (!(instance instanceof Constructor)) {
        throw new TypeError("Cannot call a class as a function");
      }
    };
    createClass = /* @__PURE__ */ function() {
      function defineProperties(target, props) {
        for (var i = 0; i < props.length; i++) {
          var descriptor = props[i];
          descriptor.enumerable = descriptor.enumerable || false;
          descriptor.configurable = true;
          if ("value" in descriptor) descriptor.writable = true;
          Object.defineProperty(target, descriptor.key, descriptor);
        }
      }
      return function(Constructor, protoProps, staticProps) {
        if (protoProps) defineProperties(Constructor.prototype, protoProps);
        if (staticProps) defineProperties(Constructor, staticProps);
        return Constructor;
      };
    }();
    _extends = Object.assign || function(target) {
      for (var i = 1; i < arguments.length; i++) {
        var source = arguments[i];
        for (var key in source) {
          if (Object.prototype.hasOwnProperty.call(source, key)) {
            target[key] = source[key];
          }
        }
      }
      return target;
    };
    inherits = function(subClass, superClass) {
      if (typeof superClass !== "function" && superClass !== null) {
        throw new TypeError("Super expression must either be null or a function, not " + typeof superClass);
      }
      subClass.prototype = Object.create(superClass && superClass.prototype, {
        constructor: {
          value: subClass,
          enumerable: false,
          writable: true,
          configurable: true
        }
      });
      if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass;
    };
    objectWithoutProperties = function(obj, keys) {
      var target = {};
      for (var i in obj) {
        if (keys.indexOf(i) >= 0) continue;
        if (!Object.prototype.hasOwnProperty.call(obj, i)) continue;
        target[i] = obj[i];
      }
      return target;
    };
    possibleConstructorReturn = function(self, call) {
      if (!self) {
        throw new ReferenceError("this hasn't been initialised - super() hasn't been called");
      }
      return call && (typeof call === "object" || typeof call === "function") ? call : self;
    };
    encodeSpecialCharacters = function encodeSpecialCharacters2(str) {
      var encode = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : true;
      if (encode === false) {
        return String(str);
      }
      return String(str).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#x27;");
    };
    getTitleFromPropsList = function getTitleFromPropsList2(propsList) {
      var innermostTitle = getInnermostProperty(propsList, TAG_NAMES.TITLE);
      var innermostTemplate = getInnermostProperty(propsList, HELMET_PROPS.TITLE_TEMPLATE);
      if (innermostTemplate && innermostTitle) {
        return innermostTemplate.replace(/%s/g, function() {
          return Array.isArray(innermostTitle) ? innermostTitle.join("") : innermostTitle;
        });
      }
      var innermostDefaultTitle = getInnermostProperty(propsList, HELMET_PROPS.DEFAULT_TITLE);
      return innermostTitle || innermostDefaultTitle || void 0;
    };
    getOnChangeClientState = function getOnChangeClientState2(propsList) {
      return getInnermostProperty(propsList, HELMET_PROPS.ON_CHANGE_CLIENT_STATE) || function() {
      };
    };
    getAttributesFromPropsList = function getAttributesFromPropsList2(tagType, propsList) {
      return propsList.filter(function(props) {
        return typeof props[tagType] !== "undefined";
      }).map(function(props) {
        return props[tagType];
      }).reduce(function(tagAttrs, current) {
        return _extends({}, tagAttrs, current);
      }, {});
    };
    getBaseTagFromPropsList = function getBaseTagFromPropsList2(primaryAttributes, propsList) {
      return propsList.filter(function(props) {
        return typeof props[TAG_NAMES.BASE] !== "undefined";
      }).map(function(props) {
        return props[TAG_NAMES.BASE];
      }).reverse().reduce(function(innermostBaseTag, tag) {
        if (!innermostBaseTag.length) {
          var keys = Object.keys(tag);
          for (var i = 0; i < keys.length; i++) {
            var attributeKey = keys[i];
            var lowerCaseAttributeKey = attributeKey.toLowerCase();
            if (primaryAttributes.indexOf(lowerCaseAttributeKey) !== -1 && tag[lowerCaseAttributeKey]) {
              return innermostBaseTag.concat(tag);
            }
          }
        }
        return innermostBaseTag;
      }, []);
    };
    getTagsFromPropsList = function getTagsFromPropsList2(tagName, primaryAttributes, propsList) {
      var approvedSeenTags = {};
      return propsList.filter(function(props) {
        if (Array.isArray(props[tagName])) {
          return true;
        }
        if (typeof props[tagName] !== "undefined") {
          warn("Helmet: " + tagName + ' should be of type "Array". Instead found type "' + _typeof(props[tagName]) + '"');
        }
        return false;
      }).map(function(props) {
        return props[tagName];
      }).reverse().reduce(function(approvedTags, instanceTags) {
        var instanceSeenTags = {};
        instanceTags.filter(function(tag) {
          var primaryAttributeKey = void 0;
          var keys2 = Object.keys(tag);
          for (var i2 = 0; i2 < keys2.length; i2++) {
            var attributeKey2 = keys2[i2];
            var lowerCaseAttributeKey = attributeKey2.toLowerCase();
            if (primaryAttributes.indexOf(lowerCaseAttributeKey) !== -1 && !(primaryAttributeKey === TAG_PROPERTIES.REL && tag[primaryAttributeKey].toLowerCase() === "canonical") && !(lowerCaseAttributeKey === TAG_PROPERTIES.REL && tag[lowerCaseAttributeKey].toLowerCase() === "stylesheet")) {
              primaryAttributeKey = lowerCaseAttributeKey;
            }
            if (primaryAttributes.indexOf(attributeKey2) !== -1 && (attributeKey2 === TAG_PROPERTIES.INNER_HTML || attributeKey2 === TAG_PROPERTIES.CSS_TEXT || attributeKey2 === TAG_PROPERTIES.ITEM_PROP)) {
              primaryAttributeKey = attributeKey2;
            }
          }
          if (!primaryAttributeKey || !tag[primaryAttributeKey]) {
            return false;
          }
          var value = tag[primaryAttributeKey].toLowerCase();
          if (!approvedSeenTags[primaryAttributeKey]) {
            approvedSeenTags[primaryAttributeKey] = {};
          }
          if (!instanceSeenTags[primaryAttributeKey]) {
            instanceSeenTags[primaryAttributeKey] = {};
          }
          if (!approvedSeenTags[primaryAttributeKey][value]) {
            instanceSeenTags[primaryAttributeKey][value] = true;
            return true;
          }
          return false;
        }).reverse().forEach(function(tag) {
          return approvedTags.push(tag);
        });
        var keys = Object.keys(instanceSeenTags);
        for (var i = 0; i < keys.length; i++) {
          var attributeKey = keys[i];
          var tagUnion = (0, import_object_assign.default)({}, approvedSeenTags[attributeKey], instanceSeenTags[attributeKey]);
          approvedSeenTags[attributeKey] = tagUnion;
        }
        return approvedTags;
      }, []).reverse();
    };
    getInnermostProperty = function getInnermostProperty2(propsList, property) {
      for (var i = propsList.length - 1; i >= 0; i--) {
        var props = propsList[i];
        if (props.hasOwnProperty(property)) {
          return props[property];
        }
      }
      return null;
    };
    reducePropsToState = function reducePropsToState2(propsList) {
      return {
        baseTag: getBaseTagFromPropsList([TAG_PROPERTIES.HREF, TAG_PROPERTIES.TARGET], propsList),
        bodyAttributes: getAttributesFromPropsList(ATTRIBUTE_NAMES.BODY, propsList),
        defer: getInnermostProperty(propsList, HELMET_PROPS.DEFER),
        encode: getInnermostProperty(propsList, HELMET_PROPS.ENCODE_SPECIAL_CHARACTERS),
        htmlAttributes: getAttributesFromPropsList(ATTRIBUTE_NAMES.HTML, propsList),
        linkTags: getTagsFromPropsList(TAG_NAMES.LINK, [TAG_PROPERTIES.REL, TAG_PROPERTIES.HREF], propsList),
        metaTags: getTagsFromPropsList(TAG_NAMES.META, [TAG_PROPERTIES.NAME, TAG_PROPERTIES.CHARSET, TAG_PROPERTIES.HTTPEQUIV, TAG_PROPERTIES.PROPERTY, TAG_PROPERTIES.ITEM_PROP], propsList),
        noscriptTags: getTagsFromPropsList(TAG_NAMES.NOSCRIPT, [TAG_PROPERTIES.INNER_HTML], propsList),
        onChangeClientState: getOnChangeClientState(propsList),
        scriptTags: getTagsFromPropsList(TAG_NAMES.SCRIPT, [TAG_PROPERTIES.SRC, TAG_PROPERTIES.INNER_HTML], propsList),
        styleTags: getTagsFromPropsList(TAG_NAMES.STYLE, [TAG_PROPERTIES.CSS_TEXT], propsList),
        title: getTitleFromPropsList(propsList),
        titleAttributes: getAttributesFromPropsList(ATTRIBUTE_NAMES.TITLE, propsList)
      };
    };
    rafPolyfill = function() {
      var clock = Date.now();
      return function(callback) {
        var currentTime = Date.now();
        if (currentTime - clock > 16) {
          clock = currentTime;
          callback(currentTime);
        } else {
          setTimeout(function() {
            rafPolyfill(callback);
          }, 0);
        }
      };
    }();
    cafPolyfill = function cafPolyfill2(id) {
      return clearTimeout(id);
    };
    requestAnimationFrame = typeof window !== "undefined" ? window.requestAnimationFrame && window.requestAnimationFrame.bind(window) || window.webkitRequestAnimationFrame || window.mozRequestAnimationFrame || rafPolyfill : global.requestAnimationFrame || rafPolyfill;
    cancelAnimationFrame = typeof window !== "undefined" ? window.cancelAnimationFrame || window.webkitCancelAnimationFrame || window.mozCancelAnimationFrame || cafPolyfill : global.cancelAnimationFrame || cafPolyfill;
    warn = function warn2(msg) {
      return console && typeof console.warn === "function" && console.warn(msg);
    };
    _helmetCallback = null;
    handleClientStateChange = function handleClientStateChange2(newState) {
      if (_helmetCallback) {
        cancelAnimationFrame(_helmetCallback);
      }
      if (newState.defer) {
        _helmetCallback = requestAnimationFrame(function() {
          commitTagChanges(newState, function() {
            _helmetCallback = null;
          });
        });
      } else {
        commitTagChanges(newState);
        _helmetCallback = null;
      }
    };
    commitTagChanges = function commitTagChanges2(newState, cb) {
      var baseTag = newState.baseTag, bodyAttributes = newState.bodyAttributes, htmlAttributes = newState.htmlAttributes, linkTags = newState.linkTags, metaTags = newState.metaTags, noscriptTags = newState.noscriptTags, onChangeClientState = newState.onChangeClientState, scriptTags = newState.scriptTags, styleTags = newState.styleTags, title = newState.title, titleAttributes = newState.titleAttributes;
      updateAttributes(TAG_NAMES.BODY, bodyAttributes);
      updateAttributes(TAG_NAMES.HTML, htmlAttributes);
      updateTitle(title, titleAttributes);
      var tagUpdates = {
        baseTag: updateTags(TAG_NAMES.BASE, baseTag),
        linkTags: updateTags(TAG_NAMES.LINK, linkTags),
        metaTags: updateTags(TAG_NAMES.META, metaTags),
        noscriptTags: updateTags(TAG_NAMES.NOSCRIPT, noscriptTags),
        scriptTags: updateTags(TAG_NAMES.SCRIPT, scriptTags),
        styleTags: updateTags(TAG_NAMES.STYLE, styleTags)
      };
      var addedTags = {};
      var removedTags = {};
      Object.keys(tagUpdates).forEach(function(tagType) {
        var _tagUpdates$tagType = tagUpdates[tagType], newTags = _tagUpdates$tagType.newTags, oldTags = _tagUpdates$tagType.oldTags;
        if (newTags.length) {
          addedTags[tagType] = newTags;
        }
        if (oldTags.length) {
          removedTags[tagType] = tagUpdates[tagType].oldTags;
        }
      });
      cb && cb();
      onChangeClientState(newState, addedTags, removedTags);
    };
    flattenArray = function flattenArray2(possibleArray) {
      return Array.isArray(possibleArray) ? possibleArray.join("") : possibleArray;
    };
    updateTitle = function updateTitle2(title, attributes) {
      if (typeof title !== "undefined" && document.title !== title) {
        document.title = flattenArray(title);
      }
      updateAttributes(TAG_NAMES.TITLE, attributes);
    };
    updateAttributes = function updateAttributes2(tagName, attributes) {
      var elementTag = document.getElementsByTagName(tagName)[0];
      if (!elementTag) {
        return;
      }
      var helmetAttributeString = elementTag.getAttribute(HELMET_ATTRIBUTE);
      var helmetAttributes = helmetAttributeString ? helmetAttributeString.split(",") : [];
      var attributesToRemove = [].concat(helmetAttributes);
      var attributeKeys = Object.keys(attributes);
      for (var i = 0; i < attributeKeys.length; i++) {
        var attribute = attributeKeys[i];
        var value = attributes[attribute] || "";
        if (elementTag.getAttribute(attribute) !== value) {
          elementTag.setAttribute(attribute, value);
        }
        if (helmetAttributes.indexOf(attribute) === -1) {
          helmetAttributes.push(attribute);
        }
        var indexToSave = attributesToRemove.indexOf(attribute);
        if (indexToSave !== -1) {
          attributesToRemove.splice(indexToSave, 1);
        }
      }
      for (var _i = attributesToRemove.length - 1; _i >= 0; _i--) {
        elementTag.removeAttribute(attributesToRemove[_i]);
      }
      if (helmetAttributes.length === attributesToRemove.length) {
        elementTag.removeAttribute(HELMET_ATTRIBUTE);
      } else if (elementTag.getAttribute(HELMET_ATTRIBUTE) !== attributeKeys.join(",")) {
        elementTag.setAttribute(HELMET_ATTRIBUTE, attributeKeys.join(","));
      }
    };
    updateTags = function updateTags2(type, tags) {
      var headElement = document.head || document.querySelector(TAG_NAMES.HEAD);
      var tagNodes = headElement.querySelectorAll(type + "[" + HELMET_ATTRIBUTE + "]");
      var oldTags = Array.prototype.slice.call(tagNodes);
      var newTags = [];
      var indexToDelete = void 0;
      if (tags && tags.length) {
        tags.forEach(function(tag) {
          var newElement = document.createElement(type);
          for (var attribute in tag) {
            if (tag.hasOwnProperty(attribute)) {
              if (attribute === TAG_PROPERTIES.INNER_HTML) {
                newElement.innerHTML = tag.innerHTML;
              } else if (attribute === TAG_PROPERTIES.CSS_TEXT) {
                if (newElement.styleSheet) {
                  newElement.styleSheet.cssText = tag.cssText;
                } else {
                  newElement.appendChild(document.createTextNode(tag.cssText));
                }
              } else {
                var value = typeof tag[attribute] === "undefined" ? "" : tag[attribute];
                newElement.setAttribute(attribute, value);
              }
            }
          }
          newElement.setAttribute(HELMET_ATTRIBUTE, "true");
          if (oldTags.some(function(existingTag, index) {
            indexToDelete = index;
            return newElement.isEqualNode(existingTag);
          })) {
            oldTags.splice(indexToDelete, 1);
          } else {
            newTags.push(newElement);
          }
        });
      }
      oldTags.forEach(function(tag) {
        return tag.parentNode.removeChild(tag);
      });
      newTags.forEach(function(tag) {
        return headElement.appendChild(tag);
      });
      return {
        oldTags,
        newTags
      };
    };
    generateElementAttributesAsString = function generateElementAttributesAsString2(attributes) {
      return Object.keys(attributes).reduce(function(str, key) {
        var attr = typeof attributes[key] !== "undefined" ? key + '="' + attributes[key] + '"' : "" + key;
        return str ? str + " " + attr : attr;
      }, "");
    };
    generateTitleAsString = function generateTitleAsString2(type, title, attributes, encode) {
      var attributeString = generateElementAttributesAsString(attributes);
      var flattenedTitle = flattenArray(title);
      return attributeString ? "<" + type + " " + HELMET_ATTRIBUTE + '="true" ' + attributeString + ">" + encodeSpecialCharacters(flattenedTitle, encode) + "</" + type + ">" : "<" + type + " " + HELMET_ATTRIBUTE + '="true">' + encodeSpecialCharacters(flattenedTitle, encode) + "</" + type + ">";
    };
    generateTagsAsString = function generateTagsAsString2(type, tags, encode) {
      return tags.reduce(function(str, tag) {
        var attributeHtml = Object.keys(tag).filter(function(attribute) {
          return !(attribute === TAG_PROPERTIES.INNER_HTML || attribute === TAG_PROPERTIES.CSS_TEXT);
        }).reduce(function(string, attribute) {
          var attr = typeof tag[attribute] === "undefined" ? attribute : attribute + '="' + encodeSpecialCharacters(tag[attribute], encode) + '"';
          return string ? string + " " + attr : attr;
        }, "");
        var tagContent = tag.innerHTML || tag.cssText || "";
        var isSelfClosing = SELF_CLOSING_TAGS.indexOf(type) === -1;
        return str + "<" + type + " " + HELMET_ATTRIBUTE + '="true" ' + attributeHtml + (isSelfClosing ? "/>" : ">" + tagContent + "</" + type + ">");
      }, "");
    };
    convertElementAttributestoReactProps = function convertElementAttributestoReactProps2(attributes) {
      var initProps = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : {};
      return Object.keys(attributes).reduce(function(obj, key) {
        obj[REACT_TAG_MAP[key] || key] = attributes[key];
        return obj;
      }, initProps);
    };
    convertReactPropstoHtmlAttributes = function convertReactPropstoHtmlAttributes2(props) {
      var initAttributes = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : {};
      return Object.keys(props).reduce(function(obj, key) {
        obj[HTML_TAG_MAP[key] || key] = props[key];
        return obj;
      }, initAttributes);
    };
    generateTitleAsReactComponent = function generateTitleAsReactComponent2(type, title, attributes) {
      var _initProps;
      var initProps = (_initProps = {
        key: title
      }, _initProps[HELMET_ATTRIBUTE] = true, _initProps);
      var props = convertElementAttributestoReactProps(attributes, initProps);
      return [import_react.default.createElement(TAG_NAMES.TITLE, props, title)];
    };
    generateTagsAsReactComponent = function generateTagsAsReactComponent2(type, tags) {
      return tags.map(function(tag, i) {
        var _mappedTag;
        var mappedTag = (_mappedTag = {
          key: i
        }, _mappedTag[HELMET_ATTRIBUTE] = true, _mappedTag);
        Object.keys(tag).forEach(function(attribute) {
          var mappedAttribute = REACT_TAG_MAP[attribute] || attribute;
          if (mappedAttribute === TAG_PROPERTIES.INNER_HTML || mappedAttribute === TAG_PROPERTIES.CSS_TEXT) {
            var content = tag.innerHTML || tag.cssText;
            mappedTag.dangerouslySetInnerHTML = { __html: content };
          } else {
            mappedTag[mappedAttribute] = tag[attribute];
          }
        });
        return import_react.default.createElement(type, mappedTag);
      });
    };
    getMethodsForTag = function getMethodsForTag2(type, tags, encode) {
      switch (type) {
        case TAG_NAMES.TITLE:
          return {
            toComponent: function toComponent() {
              return generateTitleAsReactComponent(type, tags.title, tags.titleAttributes, encode);
            },
            toString: function toString() {
              return generateTitleAsString(type, tags.title, tags.titleAttributes, encode);
            }
          };
        case ATTRIBUTE_NAMES.BODY:
        case ATTRIBUTE_NAMES.HTML:
          return {
            toComponent: function toComponent() {
              return convertElementAttributestoReactProps(tags);
            },
            toString: function toString() {
              return generateElementAttributesAsString(tags);
            }
          };
        default:
          return {
            toComponent: function toComponent() {
              return generateTagsAsReactComponent(type, tags);
            },
            toString: function toString() {
              return generateTagsAsString(type, tags, encode);
            }
          };
      }
    };
    mapStateOnServer = function mapStateOnServer2(_ref) {
      var baseTag = _ref.baseTag, bodyAttributes = _ref.bodyAttributes, encode = _ref.encode, htmlAttributes = _ref.htmlAttributes, linkTags = _ref.linkTags, metaTags = _ref.metaTags, noscriptTags = _ref.noscriptTags, scriptTags = _ref.scriptTags, styleTags = _ref.styleTags, _ref$title = _ref.title, title = _ref$title === void 0 ? "" : _ref$title, titleAttributes = _ref.titleAttributes;
      return {
        base: getMethodsForTag(TAG_NAMES.BASE, baseTag, encode),
        bodyAttributes: getMethodsForTag(ATTRIBUTE_NAMES.BODY, bodyAttributes, encode),
        htmlAttributes: getMethodsForTag(ATTRIBUTE_NAMES.HTML, htmlAttributes, encode),
        link: getMethodsForTag(TAG_NAMES.LINK, linkTags, encode),
        meta: getMethodsForTag(TAG_NAMES.META, metaTags, encode),
        noscript: getMethodsForTag(TAG_NAMES.NOSCRIPT, noscriptTags, encode),
        script: getMethodsForTag(TAG_NAMES.SCRIPT, scriptTags, encode),
        style: getMethodsForTag(TAG_NAMES.STYLE, styleTags, encode),
        title: getMethodsForTag(TAG_NAMES.TITLE, { title, titleAttributes }, encode)
      };
    };
    Helmet = function Helmet2(Component) {
      var _class, _temp;
      return _temp = _class = function(_React$Component) {
        inherits(HelmetWrapper, _React$Component);
        function HelmetWrapper() {
          classCallCheck(this, HelmetWrapper);
          return possibleConstructorReturn(this, _React$Component.apply(this, arguments));
        }
        HelmetWrapper.prototype.shouldComponentUpdate = function shouldComponentUpdate(nextProps) {
          return !(0, import_react_fast_compare.default)(this.props, nextProps);
        };
        HelmetWrapper.prototype.mapNestedChildrenToProps = function mapNestedChildrenToProps(child, nestedChildren) {
          if (!nestedChildren) {
            return null;
          }
          switch (child.type) {
            case TAG_NAMES.SCRIPT:
            case TAG_NAMES.NOSCRIPT:
              return {
                innerHTML: nestedChildren
              };
            case TAG_NAMES.STYLE:
              return {
                cssText: nestedChildren
              };
          }
          throw new Error("<" + child.type + " /> elements are self-closing and can not contain children. Refer to our API for more information.");
        };
        HelmetWrapper.prototype.flattenArrayTypeChildren = function flattenArrayTypeChildren(_ref) {
          var _babelHelpers$extends;
          var child = _ref.child, arrayTypeChildren = _ref.arrayTypeChildren, newChildProps = _ref.newChildProps, nestedChildren = _ref.nestedChildren;
          return _extends({}, arrayTypeChildren, (_babelHelpers$extends = {}, _babelHelpers$extends[child.type] = [].concat(arrayTypeChildren[child.type] || [], [_extends({}, newChildProps, this.mapNestedChildrenToProps(child, nestedChildren))]), _babelHelpers$extends));
        };
        HelmetWrapper.prototype.mapObjectTypeChildren = function mapObjectTypeChildren(_ref2) {
          var _babelHelpers$extends2, _babelHelpers$extends3;
          var child = _ref2.child, newProps = _ref2.newProps, newChildProps = _ref2.newChildProps, nestedChildren = _ref2.nestedChildren;
          switch (child.type) {
            case TAG_NAMES.TITLE:
              return _extends({}, newProps, (_babelHelpers$extends2 = {}, _babelHelpers$extends2[child.type] = nestedChildren, _babelHelpers$extends2.titleAttributes = _extends({}, newChildProps), _babelHelpers$extends2));
            case TAG_NAMES.BODY:
              return _extends({}, newProps, {
                bodyAttributes: _extends({}, newChildProps)
              });
            case TAG_NAMES.HTML:
              return _extends({}, newProps, {
                htmlAttributes: _extends({}, newChildProps)
              });
          }
          return _extends({}, newProps, (_babelHelpers$extends3 = {}, _babelHelpers$extends3[child.type] = _extends({}, newChildProps), _babelHelpers$extends3));
        };
        HelmetWrapper.prototype.mapArrayTypeChildrenToProps = function mapArrayTypeChildrenToProps(arrayTypeChildren, newProps) {
          var newFlattenedProps = _extends({}, newProps);
          Object.keys(arrayTypeChildren).forEach(function(arrayChildName) {
            var _babelHelpers$extends4;
            newFlattenedProps = _extends({}, newFlattenedProps, (_babelHelpers$extends4 = {}, _babelHelpers$extends4[arrayChildName] = arrayTypeChildren[arrayChildName], _babelHelpers$extends4));
          });
          return newFlattenedProps;
        };
        HelmetWrapper.prototype.warnOnInvalidChildren = function warnOnInvalidChildren(child, nestedChildren) {
          if (true) {
            if (!VALID_TAG_NAMES.some(function(name) {
              return child.type === name;
            })) {
              if (typeof child.type === "function") {
                return warn("You may be attempting to nest <Helmet> components within each other, which is not allowed. Refer to our API for more information.");
              }
              return warn("Only elements types " + VALID_TAG_NAMES.join(", ") + " are allowed. Helmet does not support rendering <" + child.type + "> elements. Refer to our API for more information.");
            }
            if (nestedChildren && typeof nestedChildren !== "string" && (!Array.isArray(nestedChildren) || nestedChildren.some(function(nestedChild) {
              return typeof nestedChild !== "string";
            }))) {
              throw new Error("Helmet expects a string as a child of <" + child.type + ">. Did you forget to wrap your children in braces? ( <" + child.type + ">{``}</" + child.type + "> ) Refer to our API for more information.");
            }
          }
          return true;
        };
        HelmetWrapper.prototype.mapChildrenToProps = function mapChildrenToProps(children, newProps) {
          var _this2 = this;
          var arrayTypeChildren = {};
          import_react.default.Children.forEach(children, function(child) {
            if (!child || !child.props) {
              return;
            }
            var _child$props = child.props, nestedChildren = _child$props.children, childProps = objectWithoutProperties(_child$props, ["children"]);
            var newChildProps = convertReactPropstoHtmlAttributes(childProps);
            _this2.warnOnInvalidChildren(child, nestedChildren);
            switch (child.type) {
              case TAG_NAMES.LINK:
              case TAG_NAMES.META:
              case TAG_NAMES.NOSCRIPT:
              case TAG_NAMES.SCRIPT:
              case TAG_NAMES.STYLE:
                arrayTypeChildren = _this2.flattenArrayTypeChildren({
                  child,
                  arrayTypeChildren,
                  newChildProps,
                  nestedChildren
                });
                break;
              default:
                newProps = _this2.mapObjectTypeChildren({
                  child,
                  newProps,
                  newChildProps,
                  nestedChildren
                });
                break;
            }
          });
          newProps = this.mapArrayTypeChildrenToProps(arrayTypeChildren, newProps);
          return newProps;
        };
        HelmetWrapper.prototype.render = function render() {
          var _props = this.props, children = _props.children, props = objectWithoutProperties(_props, ["children"]);
          var newProps = _extends({}, props);
          if (children) {
            newProps = this.mapChildrenToProps(children, newProps);
          }
          return import_react.default.createElement(Component, newProps);
        };
        createClass(HelmetWrapper, null, [{
          key: "canUseDOM",
          // Component.peek comes from react-side-effect:
          // For testing, you may use a static peek() method available on the returned component.
          // It lets you get the current state without resetting the mounted instance stack.
          // Don’t use it for anything other than testing.
          /**
           * @param {Object} base: {"target": "_blank", "href": "http://mysite.com/"}
           * @param {Object} bodyAttributes: {"className": "root"}
           * @param {String} defaultTitle: "Default Title"
           * @param {Boolean} defer: true
           * @param {Boolean} encodeSpecialCharacters: true
           * @param {Object} htmlAttributes: {"lang": "en", "amp": undefined}
           * @param {Array} link: [{"rel": "canonical", "href": "http://mysite.com/example"}]
           * @param {Array} meta: [{"name": "description", "content": "Test description"}]
           * @param {Array} noscript: [{"innerHTML": "<img src='http://mysite.com/js/test.js'"}]
           * @param {Function} onChangeClientState: "(newState) => console.log(newState)"
           * @param {Array} script: [{"type": "text/javascript", "src": "http://mysite.com/js/test.js"}]
           * @param {Array} style: [{"type": "text/css", "cssText": "div { display: block; color: blue; }"}]
           * @param {String} title: "Title"
           * @param {Object} titleAttributes: {"itemprop": "name"}
           * @param {String} titleTemplate: "MySite.com - %s"
           */
          set: function set$$1(canUseDOM) {
            Component.canUseDOM = canUseDOM;
          }
        }]);
        return HelmetWrapper;
      }(import_react.default.Component), _class.propTypes = {
        base: import_prop_types.default.object,
        bodyAttributes: import_prop_types.default.object,
        children: import_prop_types.default.oneOfType([import_prop_types.default.arrayOf(import_prop_types.default.node), import_prop_types.default.node]),
        defaultTitle: import_prop_types.default.string,
        defer: import_prop_types.default.bool,
        encodeSpecialCharacters: import_prop_types.default.bool,
        htmlAttributes: import_prop_types.default.object,
        link: import_prop_types.default.arrayOf(import_prop_types.default.object),
        meta: import_prop_types.default.arrayOf(import_prop_types.default.object),
        noscript: import_prop_types.default.arrayOf(import_prop_types.default.object),
        onChangeClientState: import_prop_types.default.func,
        script: import_prop_types.default.arrayOf(import_prop_types.default.object),
        style: import_prop_types.default.arrayOf(import_prop_types.default.object),
        title: import_prop_types.default.string,
        titleAttributes: import_prop_types.default.object,
        titleTemplate: import_prop_types.default.string
      }, _class.defaultProps = {
        defer: true,
        encodeSpecialCharacters: true
      }, _class.peek = Component.peek, _class.rewind = function() {
        var mappedState = Component.rewind();
        if (!mappedState) {
          mappedState = mapStateOnServer({
            baseTag: [],
            bodyAttributes: {},
            encodeSpecialCharacters: true,
            htmlAttributes: {},
            linkTags: [],
            metaTags: [],
            noscriptTags: [],
            scriptTags: [],
            styleTags: [],
            title: "",
            titleAttributes: {}
          });
        }
        return mappedState;
      }, _temp;
    };
    NullComponent = function NullComponent2() {
      return null;
    };
    HelmetSideEffects = (0, import_react_side_effect.default)(reducePropsToState, handleClientStateChange, mapStateOnServer)(NullComponent);
    HelmetExport = Helmet(HelmetSideEffects);
    HelmetExport.renderStatic = HelmetExport.rewind;
    Helmet_default = HelmetExport;
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/favicon.ico
var favicon_exports = {};
__export(favicon_exports, {
  default: () => default3
});
import { default as default3 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/favicon.ico";
import * as favicon_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/favicon.ico";
var init_favicon = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/favicon.ico"() {
    __reExport(favicon_exports, favicon_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-mask-icon.svg
var govuk_mask_icon_exports = {};
__export(govuk_mask_icon_exports, {
  default: () => default4
});
import { default as default4 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-mask-icon.svg";
import * as govuk_mask_icon_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-mask-icon.svg";
var init_govuk_mask_icon = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-mask-icon.svg"() {
    __reExport(govuk_mask_icon_exports, govuk_mask_icon_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-180x180.png
var govuk_apple_touch_icon_180x180_exports = {};
__export(govuk_apple_touch_icon_180x180_exports, {
  default: () => default5
});
import { default as default5 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-180x180.png";
import * as govuk_apple_touch_icon_180x180_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-180x180.png";
var init_govuk_apple_touch_icon_180x180 = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-180x180.png"() {
    __reExport(govuk_apple_touch_icon_180x180_exports, govuk_apple_touch_icon_180x180_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-167x167.png
var govuk_apple_touch_icon_167x167_exports = {};
__export(govuk_apple_touch_icon_167x167_exports, {
  default: () => default6
});
import { default as default6 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-167x167.png";
import * as govuk_apple_touch_icon_167x167_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-167x167.png";
var init_govuk_apple_touch_icon_167x167 = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-167x167.png"() {
    __reExport(govuk_apple_touch_icon_167x167_exports, govuk_apple_touch_icon_167x167_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-152x152.png
var govuk_apple_touch_icon_152x152_exports = {};
__export(govuk_apple_touch_icon_152x152_exports, {
  default: () => default7
});
import { default as default7 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-152x152.png";
import * as govuk_apple_touch_icon_152x152_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-152x152.png";
var init_govuk_apple_touch_icon_152x152 = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon-152x152.png"() {
    __reExport(govuk_apple_touch_icon_152x152_exports, govuk_apple_touch_icon_152x152_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon.png
var govuk_apple_touch_icon_exports = {};
__export(govuk_apple_touch_icon_exports, {
  default: () => default8
});
import { default as default8 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon.png";
import * as govuk_apple_touch_icon_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon.png";
var init_govuk_apple_touch_icon = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-apple-touch-icon.png"() {
    __reExport(govuk_apple_touch_icon_exports, govuk_apple_touch_icon_star);
  }
});

// vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-opengraph-image.png
var govuk_opengraph_image_exports = {};
__export(govuk_opengraph_image_exports, {
  default: () => default9
});
import { default as default9 } from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-opengraph-image.png";
import * as govuk_opengraph_image_star from "D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-opengraph-image.png";
var init_govuk_opengraph_image = __esm({
  "vite:dep-pre-bundle:external-conversion:D:/Users/neil.foubister/Code/CPS.ComplexCases/ui-spa/node_modules/govuk-frontend/govuk/assets/images/govuk-opengraph-image.png"() {
    __reExport(govuk_opengraph_image_exports, govuk_opengraph_image_star);
  }
});

// node_modules/govuk-react-jsx/govuk/template/index.js
var require_template = __commonJS({
  "node_modules/govuk-react-jsx/govuk/template/index.js"(exports) {
    "use strict";
    var _interopRequireDefault = require_interopRequireDefault();
    var _typeof2 = require_typeof();
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    exports.Template = Template;
    var _react = _interopRequireWildcard(require_react());
    var _reactHelmet = (init_Helmet(), __toCommonJS(Helmet_exports));
    var _favicon = _interopRequireDefault((init_favicon(), __toCommonJS(favicon_exports)));
    var _govukMaskIcon = _interopRequireDefault((init_govuk_mask_icon(), __toCommonJS(govuk_mask_icon_exports)));
    var _govukAppleTouchIcon180x = _interopRequireDefault((init_govuk_apple_touch_icon_180x180(), __toCommonJS(govuk_apple_touch_icon_180x180_exports)));
    var _govukAppleTouchIcon167x = _interopRequireDefault((init_govuk_apple_touch_icon_167x167(), __toCommonJS(govuk_apple_touch_icon_167x167_exports)));
    var _govukAppleTouchIcon152x = _interopRequireDefault((init_govuk_apple_touch_icon_152x152(), __toCommonJS(govuk_apple_touch_icon_152x152_exports)));
    var _govukAppleTouchIcon = _interopRequireDefault((init_govuk_apple_touch_icon(), __toCommonJS(govuk_apple_touch_icon_exports)));
    var _govukOpengraphImage = _interopRequireDefault((init_govuk_opengraph_image(), __toCommonJS(govuk_opengraph_image_exports)));
    var _ = require_govuk();
    function _getRequireWildcardCache(nodeInterop) {
      if (typeof WeakMap !== "function") return null;
      var cacheBabelInterop = /* @__PURE__ */ new WeakMap();
      var cacheNodeInterop = /* @__PURE__ */ new WeakMap();
      return (_getRequireWildcardCache = function _getRequireWildcardCache2(nodeInterop2) {
        return nodeInterop2 ? cacheNodeInterop : cacheBabelInterop;
      })(nodeInterop);
    }
    function _interopRequireWildcard(obj, nodeInterop) {
      if (!nodeInterop && obj && obj.__esModule) {
        return obj;
      }
      if (obj === null || _typeof2(obj) !== "object" && typeof obj !== "function") {
        return { "default": obj };
      }
      var cache = _getRequireWildcardCache(nodeInterop);
      if (cache && cache.has(obj)) {
        return cache.get(obj);
      }
      var newObj = {};
      var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor;
      for (var key in obj) {
        if (key !== "default" && Object.prototype.hasOwnProperty.call(obj, key)) {
          var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null;
          if (desc && (desc.get || desc.set)) {
            Object.defineProperty(newObj, key, desc);
          } else {
            newObj[key] = obj[key];
          }
        }
      }
      newObj["default"] = obj;
      if (cache) {
        cache.set(obj, newObj);
      }
      return newObj;
    }
    function Template(props) {
      var children = props.children, title = props.title, skipLink = props.skipLink, header = props.header, footer = props.footer, beforeContent = props.beforeContent, mainLang = props.mainLang, containerClassName = props.containerClassName, mainClassName = props.mainClassName, themeColor = props.themeColor;
      (0, _react.useEffect)(function() {
        document.documentElement.classList.add("govuk-template");
        document.body.classList.add("js-enabled", "govuk-template__body");
      }, []);
      return _react["default"].createElement(_react["default"].Fragment, null, _react["default"].createElement(_reactHelmet.Helmet, null, _react["default"].createElement("meta", {
        charset: "utf-8"
      }), _react["default"].createElement("title", null, title), _react["default"].createElement("meta", {
        name: "viewport",
        content: "width=device-width, initial-scale=1, viewport-fit=cover"
      }), _react["default"].createElement("meta", {
        name: "theme-color",
        content: "#0b0c0c"
      }), _react["default"].createElement("meta", {
        httpEquiv: "X-UA-Compatible",
        content: "IE=edge"
      }), _react["default"].createElement("link", {
        rel: "shortcut icon",
        sizes: "16x16 32x32 48x48",
        href: _favicon["default"],
        type: "image/x-icon"
      }), _react["default"].createElement("link", {
        rel: "mask-icon",
        href: _govukMaskIcon["default"],
        color: themeColor || "#0b0c0c"
      }), _react["default"].createElement("link", {
        rel: "apple-touch-icon",
        sizes: "180x180",
        href: _govukAppleTouchIcon180x["default"]
      }), _react["default"].createElement("link", {
        rel: "apple-touch-icon",
        sizes: "167x167",
        href: _govukAppleTouchIcon167x["default"]
      }), _react["default"].createElement("link", {
        rel: "apple-touch-icon",
        sizes: "152x152",
        href: _govukAppleTouchIcon152x["default"]
      }), _react["default"].createElement("link", {
        rel: "apple-touch-icon",
        href: _govukAppleTouchIcon["default"]
      }), _react["default"].createElement("meta", {
        property: "og:image",
        content: _govukOpengraphImage["default"]
      })), _react["default"].createElement(_.SkipLink, skipLink), _react["default"].createElement(_.Header, header), _react["default"].createElement("div", {
        className: "govuk-width-container ".concat(containerClassName || "")
      }, beforeContent, _react["default"].createElement("main", {
        className: "govuk-main-wrapper ".concat(mainClassName || ""),
        id: "main-content",
        role: "main",
        lang: mainLang || null
      }, children)), _react["default"].createElement(_.Footer, footer));
    }
    Template.defaultProps = {
      title: "GOV.UK - The best place to find government services and information",
      skipLink: {
        href: "#main-content",
        children: "Skip to main content"
      },
      header: {},
      footer: {},
      beforeContent: ""
    };
  }
});

// node_modules/govuk-react-jsx/govuk/index.js
var require_govuk = __commonJS({
  "node_modules/govuk-react-jsx/govuk/index.js"(exports) {
    Object.defineProperty(exports, "__esModule", {
      value: true
    });
    var _accordion = require_accordion();
    Object.keys(_accordion).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _accordion[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _accordion[key];
        }
      });
    });
    var _backLink = require_back_link();
    Object.keys(_backLink).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _backLink[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _backLink[key];
        }
      });
    });
    var _breadcrumbs = require_breadcrumbs();
    Object.keys(_breadcrumbs).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _breadcrumbs[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _breadcrumbs[key];
        }
      });
    });
    var _button = require_button();
    Object.keys(_button).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _button[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _button[key];
        }
      });
    });
    var _characterCount = require_character_count();
    Object.keys(_characterCount).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _characterCount[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _characterCount[key];
        }
      });
    });
    var _checkboxes = require_checkboxes();
    Object.keys(_checkboxes).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _checkboxes[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _checkboxes[key];
        }
      });
    });
    var _cookieBanner = require_cookie_banner();
    Object.keys(_cookieBanner).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _cookieBanner[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _cookieBanner[key];
        }
      });
    });
    var _dateInput = require_date_input();
    Object.keys(_dateInput).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _dateInput[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _dateInput[key];
        }
      });
    });
    var _details = require_details();
    Object.keys(_details).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _details[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _details[key];
        }
      });
    });
    var _errorMessage = require_error_message();
    Object.keys(_errorMessage).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _errorMessage[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _errorMessage[key];
        }
      });
    });
    var _errorSummary = require_error_summary();
    Object.keys(_errorSummary).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _errorSummary[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _errorSummary[key];
        }
      });
    });
    var _fieldset = require_fieldset();
    Object.keys(_fieldset).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _fieldset[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _fieldset[key];
        }
      });
    });
    var _fileUpload = require_file_upload();
    Object.keys(_fileUpload).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _fileUpload[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _fileUpload[key];
        }
      });
    });
    var _footer = require_footer();
    Object.keys(_footer).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _footer[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _footer[key];
        }
      });
    });
    var _header = require_header();
    Object.keys(_header).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _header[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _header[key];
        }
      });
    });
    var _hint = require_hint();
    Object.keys(_hint).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _hint[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _hint[key];
        }
      });
    });
    var _input = require_input();
    Object.keys(_input).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _input[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _input[key];
        }
      });
    });
    var _insetText = require_inset_text();
    Object.keys(_insetText).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _insetText[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _insetText[key];
        }
      });
    });
    var _label = require_label();
    Object.keys(_label).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _label[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _label[key];
        }
      });
    });
    var _notificationBanner = require_notification_banner();
    Object.keys(_notificationBanner).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _notificationBanner[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _notificationBanner[key];
        }
      });
    });
    var _panel = require_panel();
    Object.keys(_panel).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _panel[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _panel[key];
        }
      });
    });
    var _phaseBanner = require_phase_banner();
    Object.keys(_phaseBanner).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _phaseBanner[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _phaseBanner[key];
        }
      });
    });
    var _radios = require_radios();
    Object.keys(_radios).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _radios[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _radios[key];
        }
      });
    });
    var _select = require_select();
    Object.keys(_select).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _select[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _select[key];
        }
      });
    });
    var _skipLink = require_skip_link();
    Object.keys(_skipLink).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _skipLink[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _skipLink[key];
        }
      });
    });
    var _summaryList = require_summary_list();
    Object.keys(_summaryList).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _summaryList[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _summaryList[key];
        }
      });
    });
    var _table = require_table();
    Object.keys(_table).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _table[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _table[key];
        }
      });
    });
    var _tabs = require_tabs();
    Object.keys(_tabs).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _tabs[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _tabs[key];
        }
      });
    });
    var _tag = require_tag();
    Object.keys(_tag).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _tag[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _tag[key];
        }
      });
    });
    var _textarea = require_textarea();
    Object.keys(_textarea).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _textarea[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _textarea[key];
        }
      });
    });
    var _warningText = require_warning_text();
    Object.keys(_warningText).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _warningText[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _warningText[key];
        }
      });
    });
    var _template = require_template();
    Object.keys(_template).forEach(function(key) {
      if (key === "default" || key === "__esModule") return;
      if (key in exports && exports[key] === _template[key]) return;
      Object.defineProperty(exports, key, {
        enumerable: true,
        get: function get() {
          return _template[key];
        }
      });
    });
  }
});
export default require_govuk();
/*! Bundled license information:

@babel/runtime/helpers/regeneratorRuntime.js:
  (*! regenerator-runtime -- Copyright (c) 2014-present, Facebook, Inc. -- license (MIT): https://github.com/facebook/regenerator/blob/main/LICENSE *)

react-router/dist/development/dom-export.js:
  (**
   * react-router v7.2.0
   *
   * Copyright (c) Remix Software Inc.
   *
   * This source code is licensed under the MIT license found in the
   * LICENSE.md file in the root directory of this source tree.
   *
   * @license MIT
   *)

react-router/dist/development/index.js:
  (**
   * react-router v7.2.0
   *
   * Copyright (c) Remix Software Inc.
   *
   * This source code is licensed under the MIT license found in the
   * LICENSE.md file in the root directory of this source tree.
   *
   * @license MIT
   *)

react-router-dom/dist/index.js:
  (**
   * react-router-dom v7.2.0
   *
   * Copyright (c) Remix Software Inc.
   *
   * This source code is licensed under the MIT license found in the
   * LICENSE.md file in the root directory of this source tree.
   *
   * @license MIT
   *)

react-is/cjs/react-is.development.js:
  (** @license React v16.13.1
   * react-is.development.js
   *
   * Copyright (c) Facebook, Inc. and its affiliates.
   *
   * This source code is licensed under the MIT license found in the
   * LICENSE file in the root directory of this source tree.
   *)

object-assign/index.js:
  (*
  object-assign
  (c) Sindre Sorhus
  @license MIT
  *)
*/
//# sourceMappingURL=govuk-react-jsx.js.map
