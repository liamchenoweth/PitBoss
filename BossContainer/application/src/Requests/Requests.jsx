import React, { useEffect, useState } from 'react';
import { Switch, Route, Link, useParams } from "react-router-dom";

import Stop from '@material-ui/icons/Stop';

import { makeStyles } from '@material-ui/core/styles';
import Typography from '@material-ui/core/Typography';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Paginate from '../Utilities/Paginate/Paginate';
import ReactJson from 'react-json-view';

import { Get, Delete } from '../Modules/requests';
import StepPlane from '../Pipelines/Steps/StepPlane';
import Step from '../Pipelines/Steps/Step';

const styles = makeStyles(theme => ({
    stepLayout: {
        backgroundColor: "#ddd",
        maxHeight: 600,
        borderRadius: 10,
        maxWidth: "100%",
        overflowX: "scroll"
    }
}));

function RequestRow(props)
{
    return (
        <TableRow>
            <TableCell><Link to={`/requests/${props.id}`}>{props.id}</Link></TableCell>
            <TableCell><Link to={`/pipelines/${props.pipelineName}`}>{props.pipelineName}</Link></TableCell>
            <TableCell>{props.status}</TableCell>
            <TableCell>{(new Date(props.created)).toLocaleString()}</TableCell>
            <TableCell>{(new Date(props.updated)).toLocaleString()}</TableCell>
            {(props.status == "Pending" || props.status == "Executing") ? <TableCell><Stop style={{color: "red"}} onClick={CancelRequest(props.id)}/></TableCell> : <TableCell/>}
        </TableRow>
    )
}

function CancelRequest(request)
{
    return async () => {
        await Delete(`/api/requests/${request}/cancel`);
    }
}

function RequestsList()
{
    let [requests, updateRequests] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    useEffect(() => {
        (async () => {
            var requestList = (await Get("/api/requests")).data;
            updateRequests(requestList);
        })();
    }, [])
    return (
        <div>
            <Typography variant="h4">Requests</Typography>
            <br/>
            <Typography variant="body1">
                These are the requests made to your pipelines
            </Typography>
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>Id</TableCell>
                            <TableCell>Pipeline</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Created</TableCell>
                            <TableCell>Updated</TableCell>
                            <TableCell></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        <Paginate pageCount={pageRows} page={currentPage}>
                            {
                                requests.map(item => {
                                    return <RequestRow {...item} key={item.id}/>
                                })
                            }
                        </Paginate>
                        <TableRow>
                            <TablePagination 
                                rowsPerPageOptions={[10, 25, 50, 100]}
                                page={currentPage} 
                                rowsPerPage={pageRows} 
                                count={requests.length} 
                                onChangePage={(event, page) => {updateCurrentPage(page)}}
                                onChangeRowsPerPage={(event) => updatePageRows(event.target.value)}
                                />
                        </TableRow>
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    )
}

String.prototype.toHHMMSS = function () {
    var sec_num = parseInt(this, 10); // don't forget the second param
    var hours   = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = sec_num - (hours * 3600) - (minutes * 60);

    if (hours   < 10) {hours   = "0"+hours;}
    if (minutes < 10) {minutes = "0"+minutes;}
    if (seconds < 10) {seconds = "0"+seconds;}
    return hours+':'+minutes+':'+seconds;
}

function getStepColor(status)
{
    if(status == "Executing") return "orange";
    if(status == "Complete") return "green";
    if((status == "Failed" || status == "Cancelled")) return "red";
    return "grey";
}

function isObject(val) {
    if (val === null) { return false;}
    return ( (typeof val === 'function') || (typeof val === 'object') );
}

function Request()
{
    var classes = styles();
    var params = useParams();
    var [request, updateRequest] = useState({});
    var [operations, updateOperations] = useState([]);
    var [pipeline, updatePipeline] = useState();
    useEffect(() => {
        (async () => {
            var requestData = (await Get(`/api/requests/${params.id}`)).data;
            var operationData = (await Get(`/api/requests/${params.id}/operations`)).data;
            var pipelineData = (await Get(`/api/pipelines/${requestData.pipelineName}`)).data;
            updateRequest(requestData);
            updatePipeline(pipelineData);
            updateOperations(operationData);
        })();
    }, [])
    console.log(pipeline);
    return (
        <div>
            <Typography variant="h4">Request: {params.id}</Typography>
            <br/>
            {pipeline && <div className={classes.stepLayout}>
                <StepPlane>
                    {pipeline.steps.map((step, i) => {
                        console.log(step);
                        var op = operations ? operations.filter(x => x.pipelineStepId == step.id)[0] : [];
                        return (<Step {...step} key={step.id} color={getStepColor(request.status)}>
                            {op && (new Date(op.started)).getFullYear() != 1 && ((((new Date(op.completed)).getFullYear() == 1 ? Date.now() : new Date(op.completed))  - new Date(op.started)) / 1000).toString().toHHMMSS()}                            
                        </Step>)
                    })}
                </StepPlane>
            </div>}
            <br/>
            <Typography variant="h6">Input:</Typography>
            <ReactJson src={isObject(request.input) ? request.input : { input: request.input }} theme="solarized"/>
            <br/>
            {request.response && <Typography variant="h6">Output:</Typography>}
            {request.response && <ReactJson src={isObject(request.response.result) ? request.response.result : { output: request.response.result }} theme="solarized"/>}
        </div>
    )
}

function Requests()
{
    return (
        <div>
            <Switch>
                <Route path="/requests/:id">
                    <Request/>
                </Route>
                <Route exact path="/requests">
                    <RequestsList/>
                </Route>
            </Switch>
        </div>
    );
}

export default Requests;