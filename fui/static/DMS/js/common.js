$(document).ready(function () {
	"use strict";
	$(".btn_collapse").click(function () {
		$(".main_box_left,.main_box_right").toggleClass("open");
		if ($(".btn_collapse i").hasClass("bowtie-chevron-left-end")) {
			$(".btn_collapse i").removeClass("bowtie-chevron-left-end");
			$(".btn_collapse i").addClass("bowtie-chevron-right-end");
		} else {
			$(".btn_collapse i").addClass("bowtie-chevron-left-end");
			$(".btn_collapse i").removeClass("bowtie-chevron-right-end");
		}
	});
	$(".btn_menu a").click(function () {
		$(".sidebar,.main_container").toggleClass("open");
	});
	$(".switch").click(function () {
		$(this).toggleClass("change");
	});
	$(".more_function").click(function () {
		if ($(this).hasClass("mf_active")) {
			$(this).removeClass("mf_active");
		} else {
			$(".thumb_file .more_function").removeClass("mf_active");
			$(this).addClass("mf_active");
		}
	});
	//$('#myTab a').on('click', function (e) {
	//	e.preventDefault();
	//	$(this).tab('show');
	//});
});
$(document).ready(function () {
	"use strict";
	//$('.dropdown-toggle').dropdown();
});
//$(document).ready(function () {
//	"use strict";
//	debugger
//	var _wWindow = $(this).width();
//        if (_wWindow > 1366) {
//            $(".box_main_title").width($("#header").width() - $(".logo").width() - $(".btn_function").width() - 42);
//        }
//        else {
//        }
//	});
// CUSTOM SELECT
var x, i, j, selElmnt, a, b, c;
/* Look for any elements with the class "customSelect": */
x = document.getElementsByClassName("customSelect");
for (i = 0; i < x.length; i++) {
	selElmnt = x[i].getElementsByTagName("select")[0];
	/* For each element, create a new DIV that will act as the selected item: */
	a = document.createElement("DIV");
	a.setAttribute("class", "select-selected");
	a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
	x[i].appendChild(a);
	/* For each element, create a new DIV that will contain the option list: */
	b = document.createElement("DIV");
	b.setAttribute("class", "select-items select-hide");
	for (j = 1; j < selElmnt.length; j++) {
		/* For each option in the original select element,
		create a new DIV that will act as an option item: */
		c = document.createElement("DIV");
		c.innerHTML = selElmnt.options[j].innerHTML;
		c.addEventListener("click", function (e) {
			/* When an item is clicked, update the original select box,
			and the selected item: */
			var y, i, k, s, h;
			s = this.parentNode.parentNode.getElementsByTagName("select")[0];
			h = this.parentNode.previousSibling;
			for (i = 0; i < s.length; i++) {
				if (s.options[i].innerHTML == this.innerHTML) {
					s.selectedIndex = i;
					h.innerHTML = this.innerHTML;
					y = this.parentNode.getElementsByClassName("same-as-selected");
					for (k = 0; k < y.length; k++) {
						y[k].removeAttribute("class");
					}
					this.setAttribute("class", "same-as-selected");
					break;
				}
			}
			h.click();
		});
		b.appendChild(c);
	}
	x[i].appendChild(b);
	a.addEventListener("click", function (e) {
		/* When the select box is clicked, close any other select boxes,
		and open/close the current select box: */
		e.stopPropagation();
		closeAllSelect(this);
		this.nextSibling.classList.toggle("select-hide");
		this.classList.toggle("select-arrow-active");
	});
}

function closeAllSelect(elmnt) {
	/* A function that will close all select boxes in the document,
	except the current select box: */
	var x, y, i, arrNo = [];
	x = document.getElementsByClassName("select-items");
	y = document.getElementsByClassName("select-selected");
	for (i = 0; i < y.length; i++) {
		if (elmnt == y[i]) {
			arrNo.push(i)
		} else {
			y[i].classList.remove("select-arrow-active");
		}
	}
	for (i = 0; i < x.length; i++) {
		if (arrNo.indexOf(i)) {
			x[i].classList.add("select-hide");
		}
	}
}

/* If the user clicks anywhere outside the select box,
then close all select boxes: */
document.addEventListener("click", closeAllSelect);
// END CUSTOM SELECT
// FULLSCREEN
/*$("#panel-fullscreen").click(function () {
	"use strict";
	var elem = document.documentElement;
	if ($("#panel-fullscreen i").hasClass("bowtie-view-full-screen")) {
		$("#panel-fullscreen i").removeClass("bowtie-view-full-screen");
		$("#panel-fullscreen i").addClass("bowtie-view-full-screen-exit");
		if (elem.requestFullscreen) {
			elem.requestFullscreen();
		} else if (elem.mozRequestFullScreen) { 
			elem.mozRequestFullScreen();
		} else if (elem.webkitRequestFullscreen) { 
			elem.webkitRequestFullscreen();
		} else if (elem.msRequestFullscreen) { 
			elem.msRequestFullscreen();
		}
	} else {
		$("#panel-fullscreen i").addClass("bowtie-view-full-screen");
		$("#panel-fullscreen i").removeClass("bowtie-view-full-screen-exit");
		if (document.exitFullscreen) {
			document.exitFullscreen();
		} else if (document.mozCancelFullScreen) {
			document.mozCancelFullScreen();
		} else if (document.webkitExitFullscreen) {
			document.webkitExitFullscreen();
		} else if (document.msExitFullscreen) {
			document.msExitFullscreen();
		}
	}
});*/
// END FULLSCREEN
