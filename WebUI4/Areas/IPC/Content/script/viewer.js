        (function ($) {

            // Declare Model

            var PartsBookModel = Backbone.Model.extend({
                // declare the default properties, methods, and events.
                urlRoot: 'API/Catalog',
                langCode: 'en',
                location: {
                    currentChapter: null,
                    currentPage: null,
                    currentEntry: null
                }
            });



            // Declare the View

            var PartsBookView = Backbone.View.extend({
                initialize: function () {
                    this.model.bind('change:Chapter', this.render, this);
                    this.model.bind('destroy', this.remove, this);
                    this.model.bind('change:location', this.locationChanged, this);

                    $('#tocUL li.chapter').live("click", this.selectChapter);
                    $('#tocUL li.page').live("click", this.selectPage);
                    $('#partsListTable tr').live("click", this.selectEntry);
                },

                render: function () {

                    $(this.el).find('#banner').html('The book is: ' + this.model.get('ID') + ' - ' + this.model.get('Franchise'));
                    //alert(JSON.stringify(this.model.get('toc')));
                    $('#tocUL').html(this.model.get('ID'));
                    $('#tocTemplate').tmpl(this.model.get('Chapter')).appendTo('#tocUL');
                    $('#illustrationHolder').html('');
                    this.model.set({ location: {
                        currentChapter: null,
                        currentPage: null,
                        currentEntry: null
                    }
                    }, { silent: true });

                    if (this.model.get("IpcMetadata").RTL) {
                        // This catalog requires RTL rendering.
                        $(this.el).addClass("RTL");
                        //alert($(this.el).attr("class"));
                    }

                    return this;
                },



                locationChanged: function () {
                    var chapterId = this.model.get('location').currentChapter;  // retrieves the id of the chapter that changed.
                    var pageId = this.model.get('location').currentPage;  // retrieves the id of the page that changed.
                    var toc = this.model.get('Chapter'); // retrieves the collection of chapters.

                    if (chapterId == null) {
                        // clear the center and right panes.
                        $('#illustrationHolder').html('');
                        $('#partsListTable').html('');

                    } else {
                        // chapter has been identified. Retrieve the chapters.
                        var chapter = getElementFromArray(toc, chapterId, "ID");

                        if (chapterId != null) {

                            if (chapter != undefined && chapter.Page != undefined) {
                                // Build the content of the parts list with the chapter toc.

                                if (pageId == null) {
                                    // no page was selected, only a chapter.  Build the chapterTOC in the partsListTable.
                                    $('#partsListTable').html('');
                                    $('#illustrationHolder').html('');
                                    $('#chapterTocTemplate').tmpl(chapter.Page).appendTo('#partsListTable');
                                } else {
                                    // A page has been selected.  Build the parts list  in the partsListTable.
                                    var page = getElementFromArray(chapter.Page, pageId, "ID");  // Gets a page from the list of pages.
                                    if (page != undefined && page.Entry != undefined) {
                                        
                                        //Build the content of the parts list with the selected partslist.
                                        $('#partsListTable').html('');
                                        $('#partslistTemplate').tmpl(page.Entry).appendTo('#partsListTable');

                                        //Draw illustration and hotpoints.
                                        renderIllustration(page.IllustrationID, 800, 600);

                                    }
                                    else{
                                    }
                                }
                            }

                            else {
                                alert("Oops, error.  Please refresh the page.");
                            }
                        }
                    }

                    this.refreshHilites(this.model.get("location"));

                },
                selectChapter: function () {

                    // Process when clicking on a chapter in the toc.
                    var chapterId = $(this).attr('data-chapter-id');

                    // If successfully selected the chapter, update the state.
                    //// TO DO:  Don't like to referece pbModel directly.  need to find a way to refer to it as 'this'.
                    pbModel.set({ location: {
                        currentChapter: chapterId,
                        currentPage: null,
                        currentEntry: null
                    }
                    });

                },

                selectPage: function () {

                    var toc = pbModel.get('Chapter');
                    var chapterId = $(this).attr('data-chapter-id');
                    var pageId = $(this).attr('data-page-id');


                    // If successfully selected the chapter, update the state.
                    //// TO DO:  Don't like to referece pbModel directly.  need to find a way to refer to it as 'this'.
                    pbModel.set({ location: {
                        currentChapter: chapterId,
                        currentPage: pageId,
                        currentEntry: null
                    }
                    });


                    return false;  //avoids that clicking on the page also clicks on the chapter.

                },

                selectEntry: function () {
                    var pageId = $(this).attr('data-page-id');
                    var entryId = $(this).attr('data-entry-id');
                    var location = pbModel.get("location");

                    if (pageId != null) {
                        // clicked on a chapter toc.  Go to the chapter.

                        // Clear the location attribute to force a change event.  For some reason it was not firing when changing only the currentEntry.
                        pbModel.set({ location: null }, { silent: true });

                        pbModel.set({ location: {
                            currentChapter: location.currentChapter,
                            currentPage: pageId,
                            currentEntry: null
                        }
                        });
                    }
                    else if (entryId != null) {
                        // Selected an item from the parts list.
                        //// TO DO:  Don't like to reference pbModel directly.  need to find a way to refer to it as 'this'.

                        location.currentEntry = entryId;

                        // Clear the location attribute to force a change event.  For some reason it was not firing when changing only the currentEntry.
                        pbModel.set({ location: null }, { silent: true });

                        pbModel.set({ location: {
                            currentChapter: location.currentChapter,
                            currentPage: location.currentPage,
                            currentEntry: entryId
                        }
                        });
                    }
                },


                refreshHilites: function (location) {
                    // clear selection.


                    $('#tocUL li.selected').removeClass('selected');
                    // find the selected items in the TOC.


                    if (location.currentPage != null) {
                        // expand the chapter if it is not already.
                        var ul = $('#tocUL li[data-chapter-id="' + location.currentChapter + '"]').find('ul');
                        if (ul.css('display') == 'none')
                            ul.slideToggle(function () {
                                $('#toc').scrollTo(ul, 1000, { axis: 'y', offset: { left: 40} });
                            });

                        $('#tocUL li[data-page-id="' + location.currentPage + '"]').addClass('selected');
                    }

                    if (location.currentEntry != null) {
                        $('#partsListTable tr[data-entry-id="' + location.currentEntry + '"]').addClass('selected');
                    }

                }



            });




            // Initialize Model.
            window.pbModel = new PartsBookModel;
            window.pbView = new PartsBookView({ el: $("#viewer"), model: pbModel });


            $('#loadBook').click(function () {
                pbModel.set({ id: $('#search').val() }, { silent: true });

                pbModel.fetch({ data: { langCode: $('#langCode').val()} });  //  loads/reloads the object.
            });

            $('#tocUL  li.chapter  button').live("click", function () {
                $(this).nextAll("ul").slideToggle();
            });


            // Utility function to return an object from an array...
            function getElementFromArray(array, value, attributeName) {
                var returnItem = null;

                $(array).each(function (index, itm) {
                    if (itm[attributeName] == value) {
                        returnItem = itm;
                        return false;  // break;
                    }
                });

                return returnItem;
            }



            /******* Illustration Management SVG **********************/

            function renderIllustration(illustrationId, originalWidth, originalHeight) {
                var baseLineY = 0;

                var width = $('#illustrationHolder').width();
                var height = $('#illustrationHolder').height();

                var ratio = width / originalWidth;


                var mapWidth = parseInt(ratio * originalWidth);
                var mapHeight = parseInt(ratio * originalHeight) + baseLineY;
                var mapZoom = 1;

                var lastX = 0,
                lastY = baseLineY;
                var clicking = false;

                $('#illustrationHolder').css('width', mapWidth);
                $('#illustrationHolder').css('height', mapHeight);

                var paper = Raphael("illustrationHolder", mapWidth, mapHeight);
                paper.clear();
                var mapImage = paper.image("API/Illustration/" + illustrationId, 0, baseLineY, mapWidth, mapHeight);


                /* map menu */
                var mapMenuHolderWidth = 216;
                var mapMenuHolderHeight = 40;
                var mapMenuHolderX = 30;
                var mapMenuHolderY = 5;

                var mapMenuHolder = paper.rect(mapMenuHolderX, mapMenuHolderY, mapMenuHolderWidth, mapMenuHolderHeight, 12).attr({
                    fill: 'black',
                    stroke: '#aaa',
                    'stroke-width': 2,
                    opacity: 0.7
                });

                var maxIcon = paper.path("M22.646,19.307c0.96-1.583,1.523-3.435,1.524-5.421C24.169,8.093,19.478,3.401,13.688,3.399C7.897,3.401,3.204,8.093,3.204,13.885c0,5.789,4.693,10.481,10.484,10.481c1.987,0,3.839-0.563,5.422-1.523l7.128,7.127l3.535-3.537L22.646,19.307zM13.688,20.369c-3.582-0.008-6.478-2.904-6.484-6.484c0.006-3.582,2.903-6.478,6.484-6.486c3.579,0.008,6.478,2.904,6.484,6.486C20.165,17.465,17.267,20.361,13.688,20.369zM15.687,9.051h-4v2.833H8.854v4.001h2.833v2.833h4v-2.834h2.832v-3.999h-2.833V9.051z").attr({
                    fill: "#bbb",
                    stroke: "#000",
                    translation: (mapMenuHolderX + 5) + ', ' + (mapMenuHolderY + 3)
                });
                var minIcon = paper.path("M22.646,19.307c0.96-1.583,1.523-3.435,1.524-5.421C24.169,8.093,19.478,3.401,13.688,3.399C7.897,3.401,3.204,8.093,3.204,13.885c0,5.789,4.693,10.481,10.484,10.481c1.987,0,3.839-0.563,5.422-1.523l7.128,7.127l3.535-3.537L22.646,19.307zM13.688,20.369c-3.582-0.008-6.478-2.904-6.484-6.484c0.006-3.582,2.903-6.478,6.484-6.486c3.579,0.008,6.478,2.904,6.484,6.486C20.165,17.465,17.267,20.361,13.688,20.369zM8.854,11.884v4.001l9.665-0.001v-3.999L8.854,11.884z").attr({
                    fill: "#bbb",
                    stroke: "#000",
                    translation: (mapMenuHolderX + 5 + 34) + ', ' + (mapMenuHolderY + 3)
                });

                var restoreIcon = paper.path("M29.772,26.433l-7.126-7.126c0.96-1.583,1.523-3.435,1.524-5.421C24.169,8.093,19.478,3.401,13.688,3.399C7.897,3.401,3.204,8.093,3.204,13.885c0,5.789,4.693,10.481,10.484,10.481c1.987,0,3.839-0.563,5.422-1.523l7.128,7.127L29.772,26.433zM7.203,13.885c0.006-3.582,2.903-6.478,6.484-6.486c3.579,0.008,6.478,2.904,6.484,6.486c-0.007,3.58-2.905,6.476-6.484,6.484C10.106,20.361,7.209,17.465,7.203,13.885z").attr({
                    fill: "#bbb",
                    stroke: "#000",
                    translation: (mapMenuHolderX + 5 + 34 + 34) + ', ' + (mapMenuHolderY + 3)
                });

                var menuItemsMouseOver = function () { /*this.node.style.cursor = 'pointer';  */
                    this.attr({
                        fill: '#fff'
                    });
                };
                var menuItemsMouseOut = function () { /*this.node.style.cursor = 'default';  */
                    this.attr({
                        fill: '#bbb'
                    });
                };
                var menuItemsMouseDown = function () {
                    this.attr({
                        fill: '#777'
                    });
                };

                maxIcon.mouseover(menuItemsMouseOver);
                maxIcon.mouseout(menuItemsMouseOut);
                minIcon.mouseover(menuItemsMouseOver);
                minIcon.mouseout(menuItemsMouseOut);
                restoreIcon.mouseover(menuItemsMouseOver);
                restoreIcon.mouseout(menuItemsMouseOut);

                maxIcon.mousedown(menuItemsMouseDown);
                maxIcon.mouseup(menuItemsMouseOver);
                minIcon.mousedown(menuItemsMouseDown);
                minIcon.mouseup(menuItemsMouseOver);
                restoreIcon.mousedown(menuItemsMouseDown);
                restoreIcon.mouseup(menuItemsMouseOver);

                $(mapMenuHolder.node).mousemove(function (e) {
                    e.stopPropagation();
                });
                $(mapMenuHolder.node).mousedown(function (e) {
                    e.stopPropagation();
                });

                maxIcon.click(function (e) {
                    mapZoom++;
                    mapImage.scale(mapZoom, mapZoom);
                    e.stopPropagation();
                    e.preventDefault();
                });

                minIcon.click(function (e) {
                    if (mapZoom == 1) return;
                    mapZoom--;
                    mapImage.scale(mapZoom, mapZoom);
                    adjustMapEdge();
                    e.stopPropagation();
                    e.preventDefault();
                });

                restoreIcon.click(function (e) {
                    if (mapZoom == 1) return;
                    mapZoom = 1;
                    mapImage.scale(mapZoom, mapZoom);
                    adjustMapEdge();
                    e.stopPropagation();
                    e.preventDefault();
                });

                var adjustMapEdge = function () {
                    if (mapImage.attr('x') > 0) mapImage.attr({
                        'x': 0
                    });
                    if (mapImage.attr('x') < (mapWidth - mapImage.attr('width'))) mapImage.attr({
                        'x': (mapWidth - mapImage.attr('width'))
                    });

                    if (mapImage.attr('y') > baseLineY) mapImage.attr({
                        'y': baseLineY
                    });
                    if (mapImage.attr('y') < (mapHeight - mapImage.attr('height'))) mapImage.attr({
                        'y': (mapHeight - mapImage.attr('height'))
                    });
                };

                $('#illustrationHolder').mousedown(function (e) {
                    clicking = true;
                    lastX = e.pageX;
                    lastY = e.pageY;
                    $('#illustrationHolder').css('cursor', 'move');
                    e.stopPropagation();
                    e.preventDefault();
                });


                $(document).mouseup(function (e) {
                    clicking = false;
                    $('#illustrationHolder').css('cursor', 'default');
                    e.stopPropagation();
                    e.preventDefault();
                });


                $('#illustrationHolder').mousemove(function (e) {
                    if (clicking == false) return;

                    var currentMapPosX = 0,
            currentMapPosY = baseLineY;

                    if ((mapImage.attr('x') <= 0) && (mapImage.attr('x') >= (mapWidth - mapImage.attr('width')))) {
                        currentMapPosX = e.pageX - lastX;
                    }

                    if ((mapImage.attr('y') <= baseLineY) && (mapImage.attr('y') >= (mapHeight - mapImage.attr('height')))) {
                        currentMapPosY = e.pageY - lastY;
                    }

                    mapImage.translate(currentMapPosX, currentMapPosY);
                    lastX = e.pageX;
                    lastY = e.pageY;

                    if (mapImage.attr('x') > 0) mapImage.attr({
                        'x': 0
                    });
                    if (mapImage.attr('x') < (mapWidth - mapImage.attr('width'))) mapImage.attr({
                        'x': (mapWidth - mapImage.attr('width'))
                    });

                    if (mapImage.attr('y') > baseLineY) mapImage.attr({
                        'y': 0
                    });
                    if (mapImage.attr('y') < (mapHeight - mapImage.attr('height'))) mapImage.attr({
                        'y': (mapHeight - mapImage.attr('height'))
                    });
                });

                $(mapImage.node).mousedown(function (e) {
                    e.preventDefault();
                });

                $(mapImage.node).mousedown(function (e) {
                    e.preventDefault();
                });
            }

            /*************************************/

        })(jQuery);
