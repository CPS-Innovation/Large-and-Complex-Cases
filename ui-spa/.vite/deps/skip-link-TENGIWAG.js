import {
  __commonJS
} from "./chunk-FOQIPI7F.js";

// node_modules/govuk-frontend/govuk/components/skip-link/skip-link.js
var require_skip_link = __commonJS({
  "node_modules/govuk-frontend/govuk/components/skip-link/skip-link.js"(exports, module) {
    (function(global2, factory) {
      typeof exports === "object" && typeof module !== "undefined" ? module.exports = factory() : typeof define === "function" && define.amd ? define("GOVUKFrontend.SkipLink", factory) : (global2.GOVUKFrontend = global2.GOVUKFrontend || {}, global2.GOVUKFrontend.SkipLink = factory());
    })(exports, function() {
      "use strict";
      (function(undefined) {
        var detect = (
          // In IE8, defineProperty could only act on DOM elements, so full support
          // for the feature requires the ability to set a property on an arbitrary object
          "defineProperty" in Object && function() {
            try {
              var a = {};
              Object.defineProperty(a, "test", { value: 42 });
              return true;
            } catch (e) {
              return false;
            }
          }()
        );
        if (detect) return;
        (function(nativeDefineProperty) {
          var supportsAccessors = Object.prototype.hasOwnProperty("__defineGetter__");
          var ERR_ACCESSORS_NOT_SUPPORTED = "Getters & setters cannot be defined on this javascript engine";
          var ERR_VALUE_ACCESSORS = "A property cannot both have accessors and be writable or have a value";
          Object.defineProperty = function defineProperty(object, property, descriptor) {
            if (nativeDefineProperty && (object === window || object === document || object === Element.prototype || object instanceof Element)) {
              return nativeDefineProperty(object, property, descriptor);
            }
            if (object === null || !(object instanceof Object || typeof object === "object")) {
              throw new TypeError("Object.defineProperty called on non-object");
            }
            if (!(descriptor instanceof Object)) {
              throw new TypeError("Property description must be an object");
            }
            var propertyString = String(property);
            var hasValueOrWritable = "value" in descriptor || "writable" in descriptor;
            var getterType = "get" in descriptor && typeof descriptor.get;
            var setterType = "set" in descriptor && typeof descriptor.set;
            if (getterType) {
              if (getterType !== "function") {
                throw new TypeError("Getter must be a function");
              }
              if (!supportsAccessors) {
                throw new TypeError(ERR_ACCESSORS_NOT_SUPPORTED);
              }
              if (hasValueOrWritable) {
                throw new TypeError(ERR_VALUE_ACCESSORS);
              }
              Object.__defineGetter__.call(object, propertyString, descriptor.get);
            } else {
              object[propertyString] = descriptor.value;
            }
            if (setterType) {
              if (setterType !== "function") {
                throw new TypeError("Setter must be a function");
              }
              if (!supportsAccessors) {
                throw new TypeError(ERR_ACCESSORS_NOT_SUPPORTED);
              }
              if (hasValueOrWritable) {
                throw new TypeError(ERR_VALUE_ACCESSORS);
              }
              Object.__defineSetter__.call(object, propertyString, descriptor.set);
            }
            if ("value" in descriptor) {
              object[propertyString] = descriptor.value;
            }
            return object;
          };
        })(Object.defineProperty);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "bind" in Function.prototype;
        if (detect) return;
        Object.defineProperty(Function.prototype, "bind", {
          value: function bind(that) {
            var $Array = Array;
            var $Object = Object;
            var ObjectPrototype = $Object.prototype;
            var ArrayPrototype = $Array.prototype;
            var Empty = function Empty2() {
            };
            var to_string = ObjectPrototype.toString;
            var hasToStringTag = typeof Symbol === "function" && typeof Symbol.toStringTag === "symbol";
            var isCallable;
            var fnToStr = Function.prototype.toString, tryFunctionObject = function tryFunctionObject2(value) {
              try {
                fnToStr.call(value);
                return true;
              } catch (e) {
                return false;
              }
            }, fnClass = "[object Function]", genClass = "[object GeneratorFunction]";
            isCallable = function isCallable2(value) {
              if (typeof value !== "function") {
                return false;
              }
              if (hasToStringTag) {
                return tryFunctionObject(value);
              }
              var strClass = to_string.call(value);
              return strClass === fnClass || strClass === genClass;
            };
            var array_slice = ArrayPrototype.slice;
            var array_concat = ArrayPrototype.concat;
            var array_push = ArrayPrototype.push;
            var max = Math.max;
            var target = this;
            if (!isCallable(target)) {
              throw new TypeError("Function.prototype.bind called on incompatible " + target);
            }
            var args = array_slice.call(arguments, 1);
            var bound;
            var binder = function() {
              if (this instanceof bound) {
                var result = target.apply(
                  this,
                  array_concat.call(args, array_slice.call(arguments))
                );
                if ($Object(result) === result) {
                  return result;
                }
                return this;
              } else {
                return target.apply(
                  that,
                  array_concat.call(args, array_slice.call(arguments))
                );
              }
            };
            var boundLength = max(0, target.length - args.length);
            var boundArgs = [];
            for (var i = 0; i < boundLength; i++) {
              array_push.call(boundArgs, "$" + i);
            }
            bound = Function("binder", "return function (" + boundArgs.join(",") + "){ return binder.apply(this, arguments); }")(binder);
            if (target.prototype) {
              Empty.prototype = target.prototype;
              bound.prototype = new Empty();
              Empty.prototype = null;
            }
            return bound;
          }
        });
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "DOMTokenList" in this && function(x) {
          return "classList" in x ? !x.classList.toggle("x", false) && !x.className : true;
        }(document.createElement("x"));
        if (detect) return;
        (function(global2) {
          var nativeImpl = "DOMTokenList" in global2 && global2.DOMTokenList;
          if (!nativeImpl || !!document.createElementNS && !!document.createElementNS("http://www.w3.org/2000/svg", "svg") && !(document.createElementNS("http://www.w3.org/2000/svg", "svg").classList instanceof DOMTokenList)) {
            global2.DOMTokenList = function() {
              var dpSupport = true;
              var defineGetter = function(object, name, fn, configurable) {
                if (Object.defineProperty)
                  Object.defineProperty(object, name, {
                    configurable: false === dpSupport ? true : !!configurable,
                    get: fn
                  });
                else object.__defineGetter__(name, fn);
              };
              try {
                defineGetter({}, "support");
              } catch (e) {
                dpSupport = false;
              }
              var _DOMTokenList = function(el, prop) {
                var that = this;
                var tokens = [];
                var tokenMap = {};
                var length = 0;
                var maxLength = 0;
                var addIndexGetter = function(i) {
                  defineGetter(that, i, function() {
                    preop();
                    return tokens[i];
                  }, false);
                };
                var reindex = function() {
                  if (length >= maxLength)
                    for (; maxLength < length; ++maxLength) {
                      addIndexGetter(maxLength);
                    }
                };
                var preop = function() {
                  var error;
                  var i;
                  var args = arguments;
                  var rSpace = /\s+/;
                  if (args.length) {
                    for (i = 0; i < args.length; ++i)
                      if (rSpace.test(args[i])) {
                        error = new SyntaxError('String "' + args[i] + '" contains an invalid character');
                        error.code = 5;
                        error.name = "InvalidCharacterError";
                        throw error;
                      }
                  }
                  if (typeof el[prop] === "object") {
                    tokens = ("" + el[prop].baseVal).replace(/^\s+|\s+$/g, "").split(rSpace);
                  } else {
                    tokens = ("" + el[prop]).replace(/^\s+|\s+$/g, "").split(rSpace);
                  }
                  if ("" === tokens[0]) tokens = [];
                  tokenMap = {};
                  for (i = 0; i < tokens.length; ++i)
                    tokenMap[tokens[i]] = true;
                  length = tokens.length;
                  reindex();
                };
                preop();
                defineGetter(that, "length", function() {
                  preop();
                  return length;
                });
                that.toLocaleString = that.toString = function() {
                  preop();
                  return tokens.join(" ");
                };
                that.item = function(idx) {
                  preop();
                  return tokens[idx];
                };
                that.contains = function(token) {
                  preop();
                  return !!tokenMap[token];
                };
                that.add = function() {
                  preop.apply(that, args = arguments);
                  for (var args, token, i = 0, l = args.length; i < l; ++i) {
                    token = args[i];
                    if (!tokenMap[token]) {
                      tokens.push(token);
                      tokenMap[token] = true;
                    }
                  }
                  if (length !== tokens.length) {
                    length = tokens.length >>> 0;
                    if (typeof el[prop] === "object") {
                      el[prop].baseVal = tokens.join(" ");
                    } else {
                      el[prop] = tokens.join(" ");
                    }
                    reindex();
                  }
                };
                that.remove = function() {
                  preop.apply(that, args = arguments);
                  for (var args, ignore = {}, i = 0, t = []; i < args.length; ++i) {
                    ignore[args[i]] = true;
                    delete tokenMap[args[i]];
                  }
                  for (i = 0; i < tokens.length; ++i)
                    if (!ignore[tokens[i]]) t.push(tokens[i]);
                  tokens = t;
                  length = t.length >>> 0;
                  if (typeof el[prop] === "object") {
                    el[prop].baseVal = tokens.join(" ");
                  } else {
                    el[prop] = tokens.join(" ");
                  }
                  reindex();
                };
                that.toggle = function(token, force) {
                  preop.apply(that, [token]);
                  if (undefined !== force) {
                    if (force) {
                      that.add(token);
                      return true;
                    } else {
                      that.remove(token);
                      return false;
                    }
                  }
                  if (tokenMap[token]) {
                    that.remove(token);
                    return false;
                  }
                  that.add(token);
                  return true;
                };
                return that;
              };
              return _DOMTokenList;
            }();
          }
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.toggle("x", false);
            if (!e.classList.contains("x")) return;
            e.classList.constructor.prototype.toggle = function toggle(token) {
              var force = arguments[1];
              if (force === undefined) {
                var add = !this.contains(token);
                this[add ? "add" : "remove"](token);
                return add;
              }
              force = !!force;
              this[force ? "add" : "remove"](token);
              return force;
            };
          })();
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.add("a", "b");
            if (e.classList.contains("b")) return;
            var native = e.classList.constructor.prototype.add;
            e.classList.constructor.prototype.add = function() {
              var args = arguments;
              var l = arguments.length;
              for (var i = 0; i < l; i++) {
                native.call(this, args[i]);
              }
            };
          })();
          (function() {
            var e = document.createElement("span");
            if (!("classList" in e)) return;
            e.classList.add("a");
            e.classList.add("b");
            e.classList.remove("a", "b");
            if (!e.classList.contains("b")) return;
            var native = e.classList.constructor.prototype.remove;
            e.classList.constructor.prototype.remove = function() {
              var args = arguments;
              var l = arguments.length;
              for (var i = 0; i < l; i++) {
                native.call(this, args[i]);
              }
            };
          })();
        })(this);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "Document" in this;
        if (detect) return;
        if (typeof WorkerGlobalScope === "undefined" && typeof importScripts !== "function") {
          if (this.HTMLDocument) {
            this.Document = this.HTMLDocument;
          } else {
            this.Document = this.HTMLDocument = document.constructor = new Function("return function Document() {}")();
            this.Document.prototype = document;
          }
        }
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "Element" in this && "HTMLElement" in this;
        if (detect) return;
        (function() {
          if (window.Element && !window.HTMLElement) {
            window.HTMLElement = window.Element;
            return;
          }
          window.Element = window.HTMLElement = new Function("return function Element() {}")();
          var vbody = document.appendChild(document.createElement("body"));
          var frame = vbody.appendChild(document.createElement("iframe"));
          var frameDocument = frame.contentWindow.document;
          var prototype = Element.prototype = frameDocument.appendChild(frameDocument.createElement("*"));
          var cache = {};
          var shiv = function(element, deep) {
            var childNodes = element.childNodes || [], index = -1, key, value, childNode;
            if (element.nodeType === 1 && element.constructor !== Element) {
              element.constructor = Element;
              for (key in cache) {
                value = cache[key];
                element[key] = value;
              }
            }
            while (childNode = deep && childNodes[++index]) {
              shiv(childNode, deep);
            }
            return element;
          };
          var elements = document.getElementsByTagName("*");
          var nativeCreateElement = document.createElement;
          var interval;
          var loopLimit = 100;
          prototype.attachEvent("onpropertychange", function(event) {
            var propertyName = event.propertyName, nonValue = !cache.hasOwnProperty(propertyName), newValue = prototype[propertyName], oldValue = cache[propertyName], index = -1, element;
            while (element = elements[++index]) {
              if (element.nodeType === 1) {
                if (nonValue || element[propertyName] === oldValue) {
                  element[propertyName] = newValue;
                }
              }
            }
            cache[propertyName] = newValue;
          });
          prototype.constructor = Element;
          if (!prototype.hasAttribute) {
            prototype.hasAttribute = function hasAttribute(name) {
              return this.getAttribute(name) !== null;
            };
          }
          function bodyCheck() {
            if (!loopLimit--) clearTimeout(interval);
            if (document.body && !document.body.prototype && /(complete|interactive)/.test(document.readyState)) {
              shiv(document, true);
              if (interval && document.body.prototype) clearTimeout(interval);
              return !!document.body.prototype;
            }
            return false;
          }
          if (!bodyCheck()) {
            document.onreadystatechange = bodyCheck;
            interval = setInterval(bodyCheck, 25);
          }
          document.createElement = function createElement(nodeName) {
            var element = nativeCreateElement(String(nodeName).toLowerCase());
            return shiv(element);
          };
          document.removeChild(vbody);
        })();
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "document" in this && "classList" in document.documentElement && "Element" in this && "classList" in Element.prototype && function() {
          var e = document.createElement("span");
          e.classList.add("a", "b");
          return e.classList.contains("b");
        }();
        if (detect) return;
        (function(global2) {
          var dpSupport = true;
          var defineGetter = function(object, name, fn, configurable) {
            if (Object.defineProperty)
              Object.defineProperty(object, name, {
                configurable: false === dpSupport ? true : !!configurable,
                get: fn
              });
            else object.__defineGetter__(name, fn);
          };
          try {
            defineGetter({}, "support");
          } catch (e) {
            dpSupport = false;
          }
          var addProp = function(o, name, attr) {
            defineGetter(o.prototype, name, function() {
              var tokenList;
              var THIS = this, gibberishProperty = "__defineGetter__DEFINE_PROPERTY" + name;
              if (THIS[gibberishProperty]) return tokenList;
              THIS[gibberishProperty] = true;
              if (false === dpSupport) {
                var visage;
                var mirror = addProp.mirror || document.createElement("div");
                var reflections = mirror.childNodes;
                var l = reflections.length;
                for (var i = 0; i < l; ++i)
                  if (reflections[i]._R === THIS) {
                    visage = reflections[i];
                    break;
                  }
                visage || (visage = mirror.appendChild(document.createElement("div")));
                tokenList = DOMTokenList.call(visage, THIS, attr);
              } else tokenList = new DOMTokenList(THIS, attr);
              defineGetter(THIS, name, function() {
                return tokenList;
              });
              delete THIS[gibberishProperty];
              return tokenList;
            }, true);
          };
          addProp(global2.Element, "classList", "className");
          addProp(global2.HTMLElement, "classList", "className");
          addProp(global2.HTMLLinkElement, "relList", "rel");
          addProp(global2.HTMLAnchorElement, "relList", "rel");
          addProp(global2.HTMLAreaElement, "relList", "rel");
        })(this);
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = "Window" in this;
        if (detect) return;
        if (typeof WorkerGlobalScope === "undefined" && typeof importScripts !== "function") {
          (function(global2) {
            if (global2.constructor) {
              global2.Window = global2.constructor;
            } else {
              (global2.Window = global2.constructor = new Function("return function Window() {}")()).prototype = this;
            }
          })(this);
        }
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      (function(undefined) {
        var detect = function(global2) {
          if (!("Event" in global2)) return false;
          if (typeof global2.Event === "function") return true;
          try {
            new Event("click");
            return true;
          } catch (e) {
            return false;
          }
        }(this);
        if (detect) return;
        (function() {
          var unlistenableWindowEvents = {
            click: 1,
            dblclick: 1,
            keyup: 1,
            keypress: 1,
            keydown: 1,
            mousedown: 1,
            mouseup: 1,
            mousemove: 1,
            mouseover: 1,
            mouseenter: 1,
            mouseleave: 1,
            mouseout: 1,
            storage: 1,
            storagecommit: 1,
            textinput: 1
          };
          if (typeof document === "undefined" || typeof window === "undefined") return;
          function indexOf(array, element) {
            var index = -1, length = array.length;
            while (++index < length) {
              if (index in array && array[index] === element) {
                return index;
              }
            }
            return -1;
          }
          var existingProto = window.Event && window.Event.prototype || null;
          window.Event = Window.prototype.Event = function Event2(type, eventInitDict) {
            if (!type) {
              throw new Error("Not enough arguments");
            }
            var event;
            if ("createEvent" in document) {
              event = document.createEvent("Event");
              var bubbles = eventInitDict && eventInitDict.bubbles !== undefined ? eventInitDict.bubbles : false;
              var cancelable = eventInitDict && eventInitDict.cancelable !== undefined ? eventInitDict.cancelable : false;
              event.initEvent(type, bubbles, cancelable);
              return event;
            }
            event = document.createEventObject();
            event.type = type;
            event.bubbles = eventInitDict && eventInitDict.bubbles !== undefined ? eventInitDict.bubbles : false;
            event.cancelable = eventInitDict && eventInitDict.cancelable !== undefined ? eventInitDict.cancelable : false;
            return event;
          };
          if (existingProto) {
            Object.defineProperty(window.Event, "prototype", {
              configurable: false,
              enumerable: false,
              writable: true,
              value: existingProto
            });
          }
          if (!("createEvent" in document)) {
            window.addEventListener = Window.prototype.addEventListener = Document.prototype.addEventListener = Element.prototype.addEventListener = function addEventListener() {
              var element = this, type = arguments[0], listener = arguments[1];
              if (element === window && type in unlistenableWindowEvents) {
                throw new Error("In IE8 the event: " + type + " is not available on the window object. Please see https://github.com/Financial-Times/polyfill-service/issues/317 for more information.");
              }
              if (!element._events) {
                element._events = {};
              }
              if (!element._events[type]) {
                element._events[type] = function(event) {
                  var list = element._events[event.type].list, events = list.slice(), index = -1, length = events.length, eventElement;
                  event.preventDefault = function preventDefault() {
                    if (event.cancelable !== false) {
                      event.returnValue = false;
                    }
                  };
                  event.stopPropagation = function stopPropagation() {
                    event.cancelBubble = true;
                  };
                  event.stopImmediatePropagation = function stopImmediatePropagation() {
                    event.cancelBubble = true;
                    event.cancelImmediate = true;
                  };
                  event.currentTarget = element;
                  event.relatedTarget = event.fromElement || null;
                  event.target = event.target || event.srcElement || element;
                  event.timeStamp = (/* @__PURE__ */ new Date()).getTime();
                  if (event.clientX) {
                    event.pageX = event.clientX + document.documentElement.scrollLeft;
                    event.pageY = event.clientY + document.documentElement.scrollTop;
                  }
                  while (++index < length && !event.cancelImmediate) {
                    if (index in events) {
                      eventElement = events[index];
                      if (indexOf(list, eventElement) !== -1 && typeof eventElement === "function") {
                        eventElement.call(element, event);
                      }
                    }
                  }
                };
                element._events[type].list = [];
                if (element.attachEvent) {
                  element.attachEvent("on" + type, element._events[type]);
                }
              }
              element._events[type].list.push(listener);
            };
            window.removeEventListener = Window.prototype.removeEventListener = Document.prototype.removeEventListener = Element.prototype.removeEventListener = function removeEventListener() {
              var element = this, type = arguments[0], listener = arguments[1], index;
              if (element._events && element._events[type] && element._events[type].list) {
                index = indexOf(element._events[type].list, listener);
                if (index !== -1) {
                  element._events[type].list.splice(index, 1);
                  if (!element._events[type].list.length) {
                    if (element.detachEvent) {
                      element.detachEvent("on" + type, element._events[type]);
                    }
                    delete element._events[type];
                  }
                }
              }
            };
            window.dispatchEvent = Window.prototype.dispatchEvent = Document.prototype.dispatchEvent = Element.prototype.dispatchEvent = function dispatchEvent(event) {
              if (!arguments.length) {
                throw new Error("Not enough arguments");
              }
              if (!event || typeof event.type !== "string") {
                throw new Error("DOM Events Exception 0");
              }
              var element = this, type = event.type;
              try {
                if (!event.bubbles) {
                  event.cancelBubble = true;
                  var cancelBubbleEvent = function(event2) {
                    event2.cancelBubble = true;
                    (element || window).detachEvent("on" + type, cancelBubbleEvent);
                  };
                  this.attachEvent("on" + type, cancelBubbleEvent);
                }
                this.fireEvent("on" + type, event);
              } catch (error) {
                event.target = element;
                do {
                  event.currentTarget = element;
                  if ("_events" in element && typeof element._events[type] === "function") {
                    element._events[type].call(element, event);
                  }
                  if (typeof element["on" + type] === "function") {
                    element["on" + type].call(element, event);
                  }
                  element = element.nodeType === 9 ? element.parentWindow : element.parentNode;
                } while (element && !event.cancelBubble);
              }
              return true;
            };
            document.attachEvent("onreadystatechange", function() {
              if (document.readyState === "complete") {
                document.dispatchEvent(new Event("DOMContentLoaded", {
                  bubbles: true
                }));
              }
            });
          }
        })();
      }).call("object" === typeof window && window || "object" === typeof self && self || "object" === typeof global && global || {});
      function SkipLink($module) {
        this.$module = $module;
        this.$linkedElement = null;
        this.linkedElementListener = false;
      }
      SkipLink.prototype.init = function() {
        if (!this.$module) {
          return;
        }
        this.$linkedElement = this.getLinkedElement();
        if (!this.$linkedElement) {
          return;
        }
        this.$module.addEventListener("click", this.focusLinkedElement.bind(this));
      };
      SkipLink.prototype.getLinkedElement = function() {
        var linkedElementId = this.getFragmentFromUrl();
        if (!linkedElementId) {
          return false;
        }
        return document.getElementById(linkedElementId);
      };
      SkipLink.prototype.focusLinkedElement = function() {
        var $linkedElement = this.$linkedElement;
        if (!$linkedElement.getAttribute("tabindex")) {
          $linkedElement.setAttribute("tabindex", "-1");
          $linkedElement.classList.add("govuk-skip-link-focused-element");
          if (!this.linkedElementListener) {
            this.$linkedElement.addEventListener("blur", this.removeFocusProperties.bind(this));
            this.linkedElementListener = true;
          }
        }
        $linkedElement.focus();
      };
      SkipLink.prototype.removeFocusProperties = function() {
        this.$linkedElement.removeAttribute("tabindex");
        this.$linkedElement.classList.remove("govuk-skip-link-focused-element");
      };
      SkipLink.prototype.getFragmentFromUrl = function() {
        if (!this.$module.hash) {
          return false;
        }
        return this.$module.hash.split("#").pop();
      };
      return SkipLink;
    });
  }
});
export default require_skip_link();
//# sourceMappingURL=skip-link-TENGIWAG.js.map
