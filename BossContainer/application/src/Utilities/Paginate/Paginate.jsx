import React from 'react';

export default function Paginate(props) {
    var page = props.children.slice(props.pageCount * props.page, props.pageCount * (props.page + 1))
    return (
        <React.Fragment>
            {page}
        </React.Fragment>
    )
}