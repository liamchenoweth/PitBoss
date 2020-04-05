import React, { useEffect, useState } from 'react';
import { useParams, Link } from "react-router-dom";

import { makeStyles } from '@material-ui/core/styles';
import Typography from '@material-ui/core/Typography';
import StepPlane from './Steps/StepPlane';
import Step from './Steps/Step';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';

import { Get } from '../Modules/requests';
import Paginate from '../Utilities/Paginate/Paginate';

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
        <TableCell>{props.status}</TableCell>
    </TableRow>
    )
}

function Pipeline()
{
    var [details, updateDetails] = useState({});
    var [requests, updateRequests] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    var params = useParams();
    var classes = styles();

    useEffect(() => {
        (async () => {
            var pipeline = (await Get(`/api/pipelines/${params.name}`)).data;
            var requestsIncoming = (await Get(`/api/pipelines/${params.name}/requests`)).data;
            updateDetails(pipeline);
            updateRequests(requestsIncoming);
        })();
    }, []);
    return (
        <div> 
            <Typography variant="h4">{params.name}</Typography>
            <br/>
            {details.steps && <div className={classes.stepLayout}>
                <StepPlane>
                    {details.steps.map(step => {
                        return <Step {...step} key={step.id} color={"green"}/>
                    })}
                </StepPlane>
            </div>}
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>Id</TableCell>
                            <TableCell>Status</TableCell>
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

export default Pipeline;