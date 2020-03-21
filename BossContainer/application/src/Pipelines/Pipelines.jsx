import React, { useEffect, useState } from 'react';

import Run from '@material-ui/icons/PlayCircleFilled';

import Paper from '@material-ui/core/Paper';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Typography from '@material-ui/core/Typography';
import { Switch, Route, Link, useLocation } from "react-router-dom";

import { Get } from '../Modules/requests';
import { getHealthSymbol } from '../Modules/helpers';
import Pipeline from './Pipeline';
import Paginate from '../Utilities/Paginate/Paginate';

function PipelineRow(props)
{
    
    var successRate = "-";
    var finished = props.requests.filter(x => x.status === "Complete" || x.status === "Failed").length;
    if(finished != 0)
    {
        var successRate = (props.requests.filter(x => x.status === "Complete").length / finished) * 100;
    }
    var location = useLocation();
    return (
        <TableRow>
            <TableCell>
                <Link to={`${location.pathname}/${props.pipelineName}`}>
                    {props.pipelineName}
                </Link>
            </TableCell>
            <TableCell>{getHealthSymbol(props.health)}</TableCell>
            <TableCell>{props.steps.length}</TableCell>
            <TableCell>{props.requests.filter(x => x.status === "Pending").length}</TableCell>
            <TableCell>{props.requests.filter(x => x.status === "Executing").length}</TableCell>
            <TableCell>{props.requests.filter(x => x.status === "Complete" || x.status === "Failed").length}</TableCell>
            <TableCell>{successRate}%</TableCell>
            <TableCell align="center"><Run style={{color: "blue"}}/></TableCell>
        </TableRow>
    )
}

function PipelineList() {
    let [pipelines, updatePipelines] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    useEffect(didUpdate => {
        (async () => {
            if(!didUpdate){
                var pipeNames = (await Get("/api/pipelines", null)).data;
                var pipesPromise = pipeNames.map(x => {
                    return {
                        health: Get(`/api/pipelines/${x.name}/health`, null),
                        requests: Get(`/api/pipelines/${x.name}/requests`, null)
                    }
                });
                console.log(pipesPromise.length);
                var pipes = [];
                for(var i = 0; i < pipesPromise.length; i++)
                {
                    console.log(pipesPromise[i].health);
                    pipes.push({
                        ...((await pipesPromise[i].health).data),
                        requests: (await pipesPromise[i].requests).data
                    })
                }
                console.log(pipes);
                updatePipelines(pipes);
            }
        })();
    }, [])
    console.log(pipelines);
    return(
        <div>
            <Typography variant="h4">Pipelines</Typography>
            <br/>
            <Typography variant="body1">
                Here you can find all your pipelines
            </Typography>
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell rowSpan={2}>Pipeline</TableCell>
                            <TableCell rowSpan={2}>Healthy</TableCell>
                            <TableCell rowSpan={2}>Steps</TableCell>
                            <TableCell colSpan={4} rowSpan={1} align="center">Requests</TableCell>
                            <TableCell rowSpan={2}></TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell rowSpan={1}>Pending</TableCell>
                            <TableCell rowSpan={1}>Executing</TableCell>
                            <TableCell rowSpan={1}>Finished</TableCell>
                            <TableCell rowSpan={1}>Success Rate</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        <Paginate pageCount={pageRows} page={currentPage}>
                            {
                                pipelines.map(item => {
                                    return <PipelineRow {...item} key={item.pipelineName}/>
                                })
                            }
                        </Paginate>
                        <TableRow>
                            <TablePagination 
                                rowsPerPageOptions={[10, 25, 50, 100]}
                                page={currentPage} 
                                rowsPerPage={pageRows} 
                                count={pipelines.length} 
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

function Pipelines()
{
    return (
        <div>
            <Switch>
                <Route path="/pipelines/:name">
                    <Pipeline/>
                </Route>
                <Route exact path="/pipelines">
                    <PipelineList/>
                </Route>
            </Switch>
        </div>
    )
}

export default Pipelines;