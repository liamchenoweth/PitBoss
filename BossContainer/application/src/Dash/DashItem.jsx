import React from 'react';
import { Paper, Typography, Divider } from '@material-ui/core';
import makeStyles from '@material-ui/styles/makeStyles';

var styles = makeStyles(theme => ({
    title: {
        padding: 5,
        paddingLeft: 10,
        textTransform: "uppercase",
        color: "#0008",
        textAlign: "left"
    },
    paper: {
        borderWidth: 1,
        borderColor: "#0005",
        textAlign: "center"
        //width: "300px"
    },
    divider: {
        height: 2,
        backgroundColor: "#0003"
    },
    content: {
        display: "inline-block",
        textAlign: "left"
    }
}))

function Dash(props)
{
    var classes = styles();
    return (
    <Paper square className={classes.paper} elevation={4}>
        <Typography variant="subtitle1" className={classes.title}>
            {props.title}
        </Typography>
        <Divider className={classes.divider}/>
        <div className={classes.content}>
            {props.children}
        </div>
    </Paper>
    )
}

export default Dash;