// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var api = (() => {
    const
        url = '/api/posts';

    return {
        getPosts: (data, callback) => {
            const defaults = {
                order: 0,
                page: 0,
                myContent: false
            };

            var d = Object.assign({}, defaults, data);

            $.get(`${url}?order=${d.order}&page=${d.page}&myContent=${d.myContent}`, callback);
        },

        postPost: (options, callback) => $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(options),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: callback
        })
    };
})();


$(function () {
    const
        $order = $('#order'),
        $currentDateElement = $('#new-post-current-date'),
        $myContent = $('#my-content');

    var
        page = 0,
        loading = false,

        printPost = (post, prepend) => {
            var p = $(`
    <article class="card shadow-sm text-justify p-3">
        <small><time datetime="${post.publicationDate}" class="text-muted">${new Date(post.publicationDate).toLocaleDateString()}</time></small>
        <header class="h4">${post.title}</header>
        <section>${post.description}</section>
    </article>`);
            prepend ? p.prependTo("#posts") : p.appendTo("#posts");
        },

        printPosts = posts => {
            if (posts.length) posts.forEach(p=>printPost(p));
            else $('#load').hide();
            loading = false
        },

        clearPosts = () => {
            $("#posts").empty();
            page = 0;
        },

        getFilterOptions = () => {return { order: $order.val(), myContent: ($myContent[0] || { checked: false }).checked, page: page }; };

    $myContent.on('change', () => {
        clearPosts();
        if (!loading) {
            loading = true;
            api.getPosts(getFilterOptions(), printPosts);
        }
    });

    $order.on('change', () => {
        clearPosts();
        if (!loading) {
            loading = true;
            api.getPosts(getFilterOptions(), printPosts);
        }
    });

    $('#new-post-modal').on('show.bs.modal', () => {
        var currentDate = new Date();
        $currentDateElement.html(currentDate.toLocaleDateString());
        $currentDateElement.attr('datetime', currentDate.toISOString());
    });

    $('#new-post-send').on('click', () => {
        api.postPost({ title: $('#new-post-title').val(), description: $('#new-post-description').val() }, post => {
            printPost(post, !(-1 * $order.val()));
        });

        $('#new-post-title').val('');
        $('#new-post-description').val('');
        $('#new-post-modal').modal('hide');
    });

    (() => {
        const loadBtn = document.getElementById('load')
        const addNewPostBtn = document.getElementById('add-new-post')

        // Observe loadBtn
        const options = {
            // Use the whole screen as scroll area
            root: null,
            // Do not grow or shrink the root area
            rootMargin: "0px",
            // Threshold of 1.0 will fire callback when 100% of element is visible
            threshold: 1.0
        };

        const observer = new IntersectionObserver((entries) => {
            // Callback to be fired
            // Entries is a list of elements out of our targets that reported a change.
            entries.forEach((entry) => {
                // Only add to list if element is coming into view not leaving
                if (entry.isIntersecting) {
                    if (entry.target == loadBtn && !loading) {
                        loading = true;                        
                        api.getPosts(getFilterOptions(), printPosts);
                        page++;
                    }
                    else if (entry.target == addNewPostBtn) {
                        $('#add-new-post-1').hide();
                    }
                } else if (entry.target == addNewPostBtn) {
                    $('#add-new-post-1').show();
                }
            });
        }, options);

        observer.observe(loadBtn);
        if (addNewPostBtn) observer.observe(addNewPostBtn);
    })();
});
